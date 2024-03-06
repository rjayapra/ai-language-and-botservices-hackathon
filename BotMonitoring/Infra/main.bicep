@description('That name is the name of our application. It has to be unique.Type a name followed by your resource group name. (<name>-<resourceGroupName>)')
param aiServiceName string = 'AIService-${uniqueString(resourceGroup().id)}'

@description('Location for all resources.')
param location string = resourceGroup().location

param resourceName string = 'myAIService'

@allowed([
  'S0'
])
param sku string = 'S0'

resource aiServicesAccount 'Microsoft.CognitiveServices/accounts@2023-10-01-preview' = {
  name: resourceName
  location: location
  kind: 'CognitiveServices'
  sku: {
    name: 'S0'
  }
  properties: {
    apiProperties: {
      textAnalytics: {
        enabled: true
      }
    }
  } 
}

resource textAnalyticsService 'Microsoft.CognitiveServices/accounts@2023-10-01-preview' = {
   
  name: 'tas'
  location: location
  sku: {
    name: 'S0'
  }
  kind: 'TextAnalytics'
  properties: {
    //enableTextTranslation: true
    //enableSentimentAnalysis: true
    //enableNamedEntityRecognition: true
    //enableEntityLinking: true
    //enableKeyPhraseExtraction: true
    //enableLanguageDetection: true
    //enableTextAnalytics: true    
  }
  dependsOn: [
    aiServicesAccount
  ]  
}

output aiServicesAccountEndpoint string = aiServicesAccount.properties.endpoint
output textAnalyticsServiceEndpoint string = textAnalyticsService.properties.endpoint

 

