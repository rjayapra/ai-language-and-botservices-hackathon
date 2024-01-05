using Microsoft.Bot.Builder;
using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;
using OrchestrationWorkflow.OW;

namespace OrchestrationWorkflow
{
    public class OrchestrationRecognizer: IRecognizer
    {
        private readonly OWRecognizer _recognizer;
        public OrchestrationRecognizer(IConfiguration configuration)
        {
            String api = configuration["OrchestrationAPIEndPoint"];
            string key = configuration["OrchestrationAPIKey"];
            string projectName = configuration["OrchestrationProjectName"];
            string deploymentName = configuration["OrchestrationDeploymentName"];
            var orchestrationIsConfigured = !string.IsNullOrEmpty(projectName) 
                    && !string.IsNullOrEmpty(deploymentName) 
                    && !string.IsNullOrEmpty(key) 
                    && !string.IsNullOrEmpty(api);

            if (orchestrationIsConfigured)
            {
                var OWApplication = new OWApplication(
                        projectName, deploymentName, key, api);
                        
                    var recognizerOptions = new OWOptions(OWApplication) { Language = "en" };
                    _recognizer = new OWRecognizer(recognizerOptions);
            }
            else
            {
                Console.WriteLine("Orchestration is not configured. Please configure your appsettings.json file.");
                return;
            }
              
        }
        public virtual bool IsConfigured => _recognizer != null;

        public virtual async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
            => await _recognizer.RecognizeAsync(turnContext, cancellationToken);

        public virtual async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
            => await _recognizer.RecognizeAsync<T>(turnContext, cancellationToken);
       
    }

}
