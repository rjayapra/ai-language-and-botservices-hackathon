// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class UserProfileDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public UserProfileDialog(UserState userState)
            : base(nameof(UserProfileDialog))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                NameStepAsync,
                NameConfirmStepAsync,
                ContactStepAsync,
                PurposeOfVisitStepAsync,
                ConfirmStepAsync,
                SummaryStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<long>(nameof(NumberPrompt<long>), ContactPromptValidatorAsync));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt), PurposeOfVisitPromptValidatorAsync));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> NameConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like to share your contact number ?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> ContactStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                // User said "yes" so we will be prompting for the contact.
                // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter your Contact number."),
                    RetryPrompt = MessageFactory.Text("The value entered must be a valid 10 digit number"),
                };

                return await stepContext.PromptAsync(nameof(NumberPrompt<long>), promptOptions, cancellationToken);
            }
            else
            {
                // User said "no" so we will skip the next step. Give -1 as the age.
                return await stepContext.NextAsync(-1, cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> PurposeOfVisitStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["contact"] = (long)stepContext.Result;

            var msg = (long)stepContext.Values["contact"] == -1 ? "No Contact given." : $"I have your Contact as {stepContext.Values["contact"]}.";

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            var promptOptions = new PromptOptions
            {
                 Prompt = MessageFactory.Text("Please provide your reason for visit today"),
                 RetryPrompt = MessageFactory.Text("Provide a short summary of what your are looking for"),
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            
        }
        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["purposeofvisit"] = (string)stepContext.Result;

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(stepContext.Values["purposeofvisit"]+" Is this right ?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool) stepContext.Result)
            {
                // Get the current profile object from user state.
                var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

                userProfile.Name = (string)stepContext.Values["name"];
                userProfile.Contact = (long)stepContext.Values["contact"];
                userProfile.PurposeOfVisit = (string)stepContext.Values["purposeofvisit"];

                var msg = $"I have your name as {userProfile.Name}";

                if (userProfile.Contact != -1)
                {
                    msg += $" and your contact as {userProfile.Contact}";
                }

                msg += ".";

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

                if (userProfile.PurposeOfVisit != null)
                {
                    try
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please wait, whiler we find you information on " + userProfile.PurposeOfVisit), cancellationToken);
                    }
                    catch
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Could not get the reason for visit"), cancellationToken);
                    }
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thanks. Your profile will not be kept."), cancellationToken);
            }

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static Task<bool> ContactPromptValidatorAsync(PromptValidatorContext<long> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            string contact = (promptContext.Recognized.Value).ToString();
            return Task.FromResult(promptContext.Recognized.Succeeded && contact.Length > 9 );
        }

        
        private static Task<bool> PurposeOfVisitPromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            int length = promptContext.Recognized.Value.Length;
            return Task.FromResult(promptContext.Recognized.Succeeded && length > 0);
            
        }        

    }

}