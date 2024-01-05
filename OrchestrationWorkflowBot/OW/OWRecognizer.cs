// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.Language.Conversations;
using Azure.Core;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TraceExtensions;
using Newtonsoft.Json.Linq;

namespace OrchestrationWorkflow.OW
{
    /// <summary>
    /// Class for a recognizer that utilizes the OW service.
    /// </summary>
    public class OWRecognizer : IRecognizer
    {
        /// <summary>
        /// The context label for a Orchestration Workflow trace activity.
        /// </summary>
        private const string OWTraceLabel = "Orchestration Workflow Trace";

        /// <summary>
        /// Key used when adding Question Answering into to  <see cref="RecognizerResult"/> intents collection.
        /// </summary>
        public const string QuestionAnsweringMatchIntent = "QuestionAnsweringMatch";

        /// <summary>
        /// The Conversation Analysis Client instance that handles calls to the service.
        /// </summary>
        private readonly ConversationAnalysisClient _conversationsClient;

        /// <summary>
        /// Orchestration Workflow Recognizer Options
        /// </summary>
        private readonly OWOptions _options;

        /// <summary>
        /// The Orchestration WorkflowRecognizer constructor.
        /// </summary>
        public OWRecognizer(OWOptions options, ConversationAnalysisClient conversationAnalysisClient = default)
        {
            // for mocking purposes
            _conversationsClient = conversationAnalysisClient ?? new ConversationAnalysisClient(
                new Uri(options.OWApplication.Endpoint),
                new AzureKeyCredential(options.OWApplication.EndpointKey));
            _options = options;
        }

        /// <summary>
        /// The RecognizeAsync function used to recognize the intents and entities in the utterance present in the turn context. 
        /// The function uses the options provided in the constructor of the Orchestration Workflow Recognizer object.
        /// </summary>
        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return await RecognizeInternalAsync(turnContext?.Activity?.AsMessageActivity()?.Text, turnContext, cancellationToken);
        }

        /// <summary>
        /// The RecognizeAsync overload of template type T that allows the user to define their own implementation of the IRecognizerConvert class.
        /// </summary>
        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeInternalAsync(turnContext?.Activity?.AsMessageActivity()?.Text, turnContext, cancellationToken));
            return result;
        }

        private async Task<RecognizerResult> RecognizeInternalAsync(string utterance, ITurnContext turnContext, CancellationToken cancellationToken)
        {

            var request = new
            {
                analysisInput = new
                {
                    conversationItem = new
                    {
                        text = utterance,
                        id = "1",
                        participantId = "1",
                    }
                },
                parameters = new
                {
                    projectName = _options.OWApplication.ProjectName,
                    deploymentName = _options.OWApplication.DeploymentName,

                    // Use Utf16CodeUnit for strings in .NET.
                    stringIndexType = "Utf16CodeUnit",
                },
                kind = "Conversation",
            };

            var OWResponse = await _conversationsClient.AnalyzeConversationAsync(RequestContent.Create(request));
            using JsonDocument result = JsonDocument.Parse(OWResponse.ContentStream);
            var recognizerResult = RecognizerResultBuilder.BuildRecognizerResultFromOWResponse(result, utterance);

            var traceInfo = JObject.FromObject(
                new
                {
                    response = result,
                    recognizerResult,
                });

            await turnContext.TraceActivityAsync("OW Recognizer", traceInfo, nameof(OWRecognizer), OWTraceLabel, cancellationToken);
            return recognizerResult;
        }
    }
}
