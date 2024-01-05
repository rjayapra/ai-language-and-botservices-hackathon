// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;


namespace OrchestrationWorkflow.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly OrchestrationRecognizer _owRecognizer;
        protected readonly ILogger Logger;

        // Dependency injection uses this constructor to instantiate MainDialog

        public MainDialog(OrchestrationRecognizer owRecognizer, 
                        BookingDialog bookingDialog, 
                        MeetingDialog meetingDialog,
                        ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _owRecognizer = owRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(bookingDialog);
            AddDialog(meetingDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_owRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: OW is not configured. To enable all capabilities, add 'OWProjectName', 'OWDeploymentName', 'OWAPIKey' and 'OWAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var weekLaterDate = DateTime.Now.AddDays(7).ToString("MMMM d, yyyy");
            var messageText = stepContext.Options?.ToString() ?? $"What can I help you with today? \n Examples:  \n\"Book a flight from Paris to Berlin on {weekLaterDate}\"  " +
                                $"or  \n\"Tell me about Surface Book Features \" or \n \"Book a meeting with John for next Monday at conference Room\"" ;
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_owRecognizer.IsConfigured)
            {
                // ow is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance. - Default flow
                return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
            }
            // Call ow and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            var owResult = await _owRecognizer.RecognizeAsync<SchemaDefinition>(stepContext.Context, cancellationToken);

            switch (owResult.GetTopIntent().intent)
            {
                case SchemaDefinition.Intent.BookFlight:
                    // Initialize BookingDetails with any entities we may have found in the response.
                    var bookingDetails = new BookingDetails()
                    {
                        Destination = owResult.Entities.GetToCity(),
                        Origin = owResult.Entities.GetFromCity(),
                        TravelDate = owResult.Entities.GetFlightDate(),
                    };

                    // Run the BookingDialog giving it whatever details we have from the ow call, it will fill out the remainder.
                    return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);

                case SchemaDefinition.Intent.GetWeather:
                    // We haven't implemented the GetWeatherDialog so we just display a TODO message.
                    var getWeatherMessageText = "TODO: get weather flow here";
                    var getWeatherMessage = MessageFactory.Text(getWeatherMessageText, getWeatherMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(getWeatherMessage, cancellationToken);
                    break;

                case SchemaDefinition.Intent.BookMeeting:
                    // Initialize MeetingDetails with any entities we may have found in the response.
                    var meetingDetails = new MeetingDetails()
                    {
                        Attendants = owResult.Entities.GetAttendant(),
                        MeetingDate = owResult.Entities.GetMeetingDate(),
                        MeetingLocation = owResult.Entities.GetLocation(),
                    };

                    // Run the MeetingDialog giving it whatever details we have from the ow call, it will fill out the remainder.
                    return await stepContext.BeginDialogAsync(nameof(MeetingDialog), meetingDetails, cancellationToken);

                case SchemaDefinition.Intent.Surface_QA:
                    string getSurfaceMessageText = owResult.Text;
                    var getSurfaceMessage = MessageFactory.Text(getSurfaceMessageText, getSurfaceMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(getSurfaceMessage, cancellationToken);
                    break;

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {owResult.GetTopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
            // the Result here will be null.
            if (stepContext.Result is BookingDetails result)
            {
                // Now we have all the booking details call the booking service.

                // If the call to the booking service was successful tell the user.

                var timeProperty = new TimexProperty(result.TravelDate);
                var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
                var messageText = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }  
            else  if (stepContext.Result is MeetingDetails meetingResults)
            {
                  //Set the meeting details
                    var messageText = $"I have your meeting set to {meetingResults.MeetingDate} Attended by {meetingResults.Attendants} at {meetingResults.MeetingLocation}";
                    var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(message, cancellationToken);            
            }

            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }
    }
}
