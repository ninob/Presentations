using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Acme.Flux.Monitor
{
    /// <summary>
    /// Class implementing IEventProcessor to 
    /// </summary>
    /// <remarks>
    /// IEventProcess requires implementing OpenAsync, ProcessEventsAsync, CloseAsync 
    /// </remarks>
    public class FluxEventProcessor : IEventProcessor
    {
        IDictionary<string, int> map;
        PartitionContext partitionContext;
        Stopwatch checkpointStopWatch;
        private static DocumentClient client;
        private string endpointUrl = string.Empty;
        private string authorizationKey = string.Empty;
        private string databaseId = string.Empty;
        private string collectionId = string.Empty;
        
        public FluxEventProcessor()
        {
            this.map = new Dictionary<string, int>();

            endpointUrl = ConfigurationManager.AppSettings["DocumentDBEndpoint"];
            authorizationKey = ConfigurationManager.AppSettings["DocumentDBAuthorizationKey"];
            databaseId = ConfigurationManager.AppSettings["flux"];
            collectionId = ConfigurationManager.AppSettings["fluxdemo"];
        }

        public Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine(string.Format("FluxEventProcessor initialize.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset));
            this.partitionContext = context;
            this.checkpointStopWatch = new Stopwatch();
            this.checkpointStopWatch.Start();
            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> events)
        {
            try
            {
                foreach (EventData eventData in events)
                {
                    string data = Encoding.UTF8.GetString(eventData.GetBytes());                    
                    Console.WriteLine(string.Format("Message received.  Partition: '{0}', Data: '{1}'", context.Lease.PartitionId,data));

                    //Save message to DocumentDB database.
                    using (client = new DocumentClient(new Uri(endpointUrl), authorizationKey))
                    {
                        SaveFluxData(databaseId, collectionId, data).Wait();
                    }
                }

                //Call checkpoint every 2 minutes, so that worker can resume processing from the 2 minutes back if it restarts.
                if (this.checkpointStopWatch.Elapsed > TimeSpan.FromMinutes(2))
                {
                    await context.CheckpointAsync();
                    lock (this)
                    {
                        this.checkpointStopWatch.Reset();
                    }
                }
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
            }
            catch (Exception exp)
            {
                Console.WriteLine("Error in processing: " + exp.Message);
            }
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine(string.Format("Processor Shuting Down.  Partition '{0}', Reason: '{1}'.", this.partitionContext.Lease.PartitionId, reason.ToString()));
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }

        /// <summary>
        /// Calls helper methods to connect and save data to DocumentDB database.
        /// </summary>
        private static async Task SaveFluxData(string databaseId, string collectionId, string fluxData)
        {
            var database = await GetOrCreateDatabaseAsync(databaseId);
            var collection = await GetOrCreateCollectionAsync(database.SelfLink, collectionId);
            var doc = await InsertDocAsync(collection.SelfLink, fluxData);
        }

    #region DocumentDB Helper Methods
        private static async Task<Database> GetOrCreateDatabaseAsync(string id)
        {
            Database database = client.CreateDatabaseQuery().Where(db => db.Id == id).ToArray().FirstOrDefault();
            if (database == null)
            {
                database = await client.CreateDatabaseAsync(new Database { Id = id });
            }

            Console.WriteLine("Created database:" + database.Id);
            return database;
        }

        private static async Task<DocumentCollection> GetOrCreateCollectionAsync(string dbLink, string id)
        {
            DocumentCollection collection = client.CreateDocumentCollectionQuery(dbLink).Where(c => c.Id == id).ToArray().FirstOrDefault();
            if (collection == null)
            {
                collection = await client.CreateDocumentCollectionAsync(dbLink, new DocumentCollection { Id = id });
            }

            Console.WriteLine("Created collection:" + collection.Id);
            return collection;
        }
        private static async Task<int> InsertDocAsync(string collSelfLink, string fluxSerializedData)
        {
            object parsedData = JsonConvert.DeserializeObject(fluxSerializedData);

            // Create a document
            Document createdDoc = await client.CreateDocumentAsync(collSelfLink, parsedData);
            Console.WriteLine("Inserted document: " + createdDoc);

            return 0;
        }
#endregion
    }
}
