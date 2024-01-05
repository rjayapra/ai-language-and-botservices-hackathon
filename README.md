# Azure AI Language Service with Bot Framework

Welcome to Azure AI Language Services with Bots workshop. 
 
In this OpenHack, you’ll go through tasks that will help you master using Language services (CQA, CLU and Orchestration workflow ) and also build bots to connect to these services. This exercice can be done alone or in group and will take 4 hours. If you find any issue or have any remark, please do open a issue in the repository.


## Language Services
Azure AI Language is a cloud-based service that provides Natural Language Processing (NLP) features for understanding and analyzing text. Use this service to help build intelligent applications using the web-based Language Studio, REST APIs, and client libraries.

Available Features 

* Named Entity Recognition
* Personally identifying (PII) and health (PHI) information detection
* Language detection
* Sentiment Analysis and opinion mining
* Summarization
* Key phrase extraction
* Entity linking
* Text analytics for health
* Custom text classification
* Custom Named Entity Recognition (Custom NER)
* Conversational language understanding
* Orchestration workflow
* Question answering
* Custom text analytics for health

### Migrate from Text Analytics, QnA Maker, or Language Understanding (LUIS)
Azure AI Language unifies three individual language services in Azure AI services - Text Analytics, QnA Maker, and Language Understanding (LUIS). If you have been using these three services, you can easily migrate to the new Azure AI Language. For instructions see [Migrate to Azure AI Language].

### Create a Bot with the Bot Framework SDK 
Bots are software agents that can participate in conversational dialogs with human users. The Microsoft Bot Framework provides a comprehensive platform for building bots that can be delivered as cloud services through the Azure Bot Service.

## Pre Requisites

### Tools

1. an Azure subscription (and at least a dedicated resource group).
1. if you plan to run command from your computer: install [Az cli] or [Azure Powershell]
1. setup [BotFramework Emulator]
1. setup Git 
1. setup VS Code

### Clone Repository

To install Git, follow these steps:

1. Visit the official Git website at https://git-scm.com/downloads.
1. Choose the appropriate installer for your operating system (Windows, macOS, or Linux).
1. Run the installer and follow the on-screen instructions.
Note: If you are using Visual Studio Code, Git is often bundled with the editor, so you may not need to install it separately.

Open the palette (SHIFT+CTRL+P) and run a Git: Clone command to clone the repository https://github.com/rjayapra/ai-services-hackathon to a local folder 
When the repository has been cloned, open the folder in Visual Studio code.
Wait while additional files are installed to support the C# code projects in the repo.

Note: If you are prompted to add required assets to build and debug, select Not Now.

### Create Language Resource:

1. Login to Azure Portal
1. Navigate to https://portal.azure.com/#create/Microsoft.CognitiveServicesTextAnalytics
1. Select  custom features "Customer Question Answering"
1. Select appropriate subscription, resource group (create one if not exists), region, provide a name and pricing tier
1. For CQA select Azure search region set as "East US" and pricing tier as "Basic"
1. Read Responsible AI Note and click "Review + create".

Use Language Studio to try out the features without any code : [Language Studio]


## Lab Exercises:

Module 1: Create a Custom Question Answering project and build a bot to use the service. We will also deploy the bot to Azure.
[BotWithCQA](BotWithCQA)

Module 2: Create a Custom Language Understanding project and build a bot to use the service. We will also deploy the bot to Azure.
[CoreBotWithCLU](CoreBotWithCLU)

Module 3: Create a Orchestration workflow project and use the CQA and CLU projects to train the model and create a deployment. Build a bot to use the service. We will also deploy the bot to Azure.
[OrchestrationWorkflowBot](OrchestrationWorkflowBot)

[Migrate to Azure AI Language]: https://learn.microsoft.com/en-us/azure/ai-services/language-service/concepts/migrate 
[Language Studio]: https://learn.microsoft.com/en-us/azure/ai-services/language-service/language-studio
[Az cli]: https://learn.microsoft.com/en-us/cli/azure/install-azure-cli
[Azure Powershell]: https://learn.microsoft.com/en-us/powershell/azure/install-azure-powershell?view=azps-11.1.0
[BotFramework Emulator]: https://github.com/Microsoft/BotFramework-Emulator

