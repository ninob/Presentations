This project was created using Visual Studio 2013. NuGet package restore is enabled.

************************************************************************************************
To run code as-is, requires provisioning of:
1) Azure Event Hub
2) Azure Blob Storage
3) Azure DocumentDB database
4) Inserting connection information for the above into the app.config <appSettings> section
*************************************************************************************************


An activityType of 'Send' will cause the program to send messages to the defined EventHub in Azure.
An activityType of 'Receive' will cause the program to received messages from the defined EventHub in Azure.

The FluxEventProcessor will save received messages to the DocumentDB database by default. 
If you don't want it to do this, comment out lines 66 - 69 in FluxEventProcessor.cs


Commandline:
--e <eventHubName> --m <messageCount> --a <activityType>

Examples:
--e flux --m 100 --a Send
--e flux --a Receive

To run from the Visual Studio debugger:
1) Open project properties
2) Navigate to the Debug section
3) Enter in the commandline in the Start Options | Command line arguments box


This code based off of Microsoft getting started sample located at: https://code.msdn.microsoft.com/windowsapps/Service-Bus-Event-Hub-286fd097/view/Discussions#content