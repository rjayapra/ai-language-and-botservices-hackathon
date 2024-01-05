// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace OrchestrationWorkflow.Dialogs
{
    public class MeetingDialog : CancelAndHelpDialog
    {
        private const string MeetingStepMsgText = "When would you like to set the meeting ?";
        private const string AttendantStepMsgText = "Who all will be participating in the meeting?";
        private const string LocationStepMsgText = "Which location do you want to set the meeting?";

        public MeetingDialog()
            : base(nameof(MeetingDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                MeetingStepAsync,
                AttendantStepAsync,
                MeetingLocationStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> MeetingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var meetingDetails = (MeetingDetails)stepContext.Options;

            if (meetingDetails.MeetingDate == null)
            {
                var promptMessage = MessageFactory.Text(MeetingStepMsgText, MeetingStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(meetingDetails.MeetingDate, cancellationToken);
        }

        private async Task<DialogTurnResult> AttendantStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var meetingDetails = (MeetingDetails)stepContext.Options;

            if (meetingDetails.Attendants == null)
            {
                var promptMessage = MessageFactory.Text(AttendantStepMsgText, AttendantStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(meetingDetails.Attendants, cancellationToken);
        }

        private async Task<DialogTurnResult> MeetingLocationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var meetingDetails = (MeetingDetails)stepContext.Options;

            if (meetingDetails.MeetingLocation == null )
            {
                var promptMessage = MessageFactory.Text(MeetingStepMsgText, MeetingStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken)
;            }

            return await stepContext.NextAsync(meetingDetails.MeetingLocation, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var meetingDetails = (MeetingDetails)stepContext.Options;

            var messageText = $"Please confirm, I have your meeting set to: {meetingDetails.MeetingDate} , Attended by: {meetingDetails.Attendants} at {meetingDetails.MeetingLocation}. Is this correct?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var meetingDetails = (MeetingDetails)stepContext.Options;

                return await stepContext.EndDialogAsync(meetingDetails, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }
    }
}
