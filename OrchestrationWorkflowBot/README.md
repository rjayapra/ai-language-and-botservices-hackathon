# Orchestration Workflow Sample

## Getting Started

1. Complete Module 1 which creates a Question Answering project
2. Complete Module 2 to create a CLU project and train a model.
3. In Language Studio, go to the **Orchestration workflow** service.
4. Select Create new project. Use the name Orchestrator and the language English before clicking next then done.
5. Once the project is created, select Add in the Schema definition page.
6. Select Yes, I want to connect it to an existing project. Add the intent name **BookFlight** and select Conversational Language Understanding as the connected service. Select the recently created FlightBooking project for the project name before clicking on Add Intent.
7. Create an Orchestration workflow project. Refer [tutorial] in the Orchestration workflow documentation.
8. Add another intent with name **'Surface_QA'** but now select Question Answering as the service and select Question answering project name.
9. Similar to conversational language understanding, go to Training jobs and start a new training job with the name **v1** and press Train.
10. Once training is complete, click to Deploying a model on the left. Select Add deployment and create a new deployment with the name **Testing**, and assign model v1 to the deployment and press Next.
11. On the next page, select the deployment name Testing for the **BookFlight**. This tells the orchestrator to call the Testing deployment in FlightBooking Project when it routes to it. Custom question answering projects only have one deployment by default.
12. Now your orchestration project is ready to be used. Any incoming request will be routed to either **BookFlight** and the FlightBooking Project in CLU or **Surface_QA** and the surface book knowledge base.

### Connect your bot to the project

Refer: A sample .NET solution that calls the [Orchestration workflow] service using the Conversations SDK. 

Follow these steps to update [appsettings.json](appsettings.json):
- In the [Azure Portal][Azure], go to your resource.
- Under Resource Management, go to `Keys and Endpoint`.
- Copy and paste the following values into their respective variables in [appsettings.json](appsettings.json):
     - One of the keys: `OrchestrationAPIKey` 
     - Endpoint: `OrchestrationAPIEndPoint`       
- `OrchestrationProjectName` is the name of the orchestration project created in [Language Studio][LS].
- `OrchestrationDeploymentName` is the name of the Deployment - (eg., Testing)

### Try this sample

- Install the Bot Framework Emulator version 4.14.0 or greater from [here][BFE]
- Clone this repository

    ```bash
    git clone https://github.com/rjayapra/ai-services-hackathon/
    ```

- In a terminal, navigate to `OrchestrationWorkflowBot`
- Run the bot, either from a terminal or from Visual Studio, using the appropriate steps below:

  A) From a terminal

  ```bash
  # run the bot
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `OrchestrationWorkflowBot` folder
  - Select `OrchestrationWorkflowBot.csproj` file
  - Press `F5` to run the project
    
  C) Connect to the bot using Bot Framework Emulator
  
  - Launch Bot Framework Emulator
  - File -> Open Bot
  - Enter a Bot URL of `http://localhost:3978/api/messages`

### Try Conversation
- Try the following utterances:
  1. Surafce Book Storage option
  2. Book a flight to Toronto from Montreal on Jan 15, 2024

### Deploy the bot to Azure
See [Deploy your C# bot to Azure][50] for instructions.

The deployment process assumes you have an account on Microsoft Azure and are able to log into the [Microsoft Azure Portal][Azure]. If you have completed Module 1 or 2 , you can use the created resource to deploy your bot.

## Further reading

- [How bots work](https://docs.microsoft.com/azure/bot-service/bot-builder-basics)
- [Question Answering Documentation](https://docs.microsoft.com/azure/cognitive-services/language-service/question-answering/overview)
- [Orchestration Workflow Documentation](https://learn.microsoft.com/en-us/azure/ai-services/language-service/orchestration-workflow/overview)
- [Channels and Bot Connector Service](https://docs.microsoft.com/azure/bot-service/bot-concepts)

[tutorial]: https://learn.microsoft.com/en-us/azure/ai-services/language-service/orchestration-workflow/tutorials/connect-services#create-an-orchestration-workflow-project
[Orchestration workflow]:  https://docs.microsoft.com/en-us/azure/cognitive-services/language-service/orchestration-workflow/overview
[Quickstart]: https://docs.microsoft.com/azure/cognitive-services/language-service/question-answering/quickstart/sdk
[BFE]: https://github.com/Microsoft/BotFramework-Emulator/releases
