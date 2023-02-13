using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using PluralsightBot.Models;
using PluralsightBot.Services;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Text.RegularExpressions;

namespace PluralsightBot.Dialogs
{
    public class BugReportDialog : ComponentDialog
    {

        private readonly BotStateService _stateService;

        public BugReportDialog(string dialogId, BotStateService stateService) : base(dialogId)
        {
            _stateService = stateService;

            InitializeWaterfallDialog();
        }

        void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                DescriptionStepAsync,
                CallbackTimeStepAsync,
                PhoneNumberStepAsync,
                BugStepAsync,
                SummaryStepAsync
            };

            AddDialog(new WaterfallDialog($"{nameof(BugReportDialog)}.mainflow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.description"));
            AddDialog(new DateTimePrompt($"{nameof(BugReportDialog)}.callbackTime", CallbackTimeValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.phoneNumber", PhoneNumberValidatorAsync));
            AddDialog(new ChoicePrompt($"{nameof(BugReportDialog)}.bug"));

            InitialDialogId = $"{nameof(BugReportDialog)}.mainflow";
        }

        private async Task<DialogTurnResult> DescriptionStepAsync(WaterfallStepContext waterfallStepContext, CancellationToken cancellationToken)
        {

            return await waterfallStepContext.PromptAsync($"{nameof(BugReportDialog)}.description", new PromptOptions
            {
                Prompt = MessageFactory.Text("Enter a description for a report")
            }, cancellationToken);

        }

        private async Task<DialogTurnResult> CallbackTimeStepAsync(WaterfallStepContext waterfallStepContext, CancellationToken cancellationToken)
        {

            waterfallStepContext.Values["description"] = (string)waterfallStepContext.Result;

            return await waterfallStepContext.PromptAsync($"{nameof(BugReportDialog)}.callbackTime", new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter in a callback time"),
                RetryPrompt = MessageFactory.Text("The value entered must be between the hours of 9am to 5pm."),
            }, cancellationToken);

        }

        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext waterfallStepContext, CancellationToken cancellationToken)
        {

            waterfallStepContext.Values["callbackTime"] = Convert.ToDateTime(((IList<DateTimeResolution>)waterfallStepContext.Result).FirstOrDefault().Value);

            return await waterfallStepContext.PromptAsync($"{nameof(BugReportDialog)}.phoneNumber", new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter in a phone number that we can call you back at."),
                RetryPrompt = MessageFactory.Text("Please enter a valid phone number"),
            }, cancellationToken);

        }

        private async Task<DialogTurnResult> BugStepAsync(WaterfallStepContext waterfallStepContext, CancellationToken cancellationToken)
        {

            waterfallStepContext.Values["phoneNumber"] = (string)waterfallStepContext.Result;

            return await waterfallStepContext.PromptAsync($"{nameof(BugReportDialog)}.bug", new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter the bug type:"),
                Choices = ChoiceFactory.ToChoices(new List<string>() { "Security", "Crash", "Power", "Perfomance", "Usability", "Serious bug", "Report" })
            }, cancellationToken);

        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext waterfallStepContext, CancellationToken cancellationToken)
        {

            waterfallStepContext.Values["bug"] = ((FoundChoice)waterfallStepContext.Result).Value;

            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(waterfallStepContext.Context, () => new());


            userProfile.Description = (string)waterfallStepContext.Values["description"];
            userProfile.PhoneNumber = (string)waterfallStepContext.Values["phoneNumber"];
            userProfile.Bug = (string)waterfallStepContext.Values["bug"];
            userProfile.CallbackTime = (DateTime)waterfallStepContext.Values["callbackTime"];

            await waterfallStepContext.Context.SendActivityAsync(MessageFactory.Text("This is the summary of your bug report"), cancellationToken);
            await waterfallStepContext.Context.SendActivityAsync(MessageFactory.Text($"Description: {userProfile.Description}"), cancellationToken);
            await waterfallStepContext.Context.SendActivityAsync(MessageFactory.Text($"Callback Time: {userProfile.CallbackTime.ToShortTimeString()}"), cancellationToken);
            await waterfallStepContext.Context.SendActivityAsync(MessageFactory.Text($"Phone Number: {userProfile.PhoneNumber}"), cancellationToken);
            await waterfallStepContext.Context.SendActivityAsync(MessageFactory.Text($"Bug Typer: {userProfile.Bug}"), cancellationToken);


            await _stateService.UserProfileAccessor.SetAsync(waterfallStepContext.Context, userProfile);

            return await waterfallStepContext.EndDialogAsync(null, cancellationToken);

        }


        private Task<bool> CallbackTimeValidatorAsync(PromptValidatorContext<IList<DateTimeResolution>> promptValidatorContext, CancellationToken cancellationToken)
        {

            var valid = false;

            if (promptValidatorContext.Recognized.Succeeded)
            {
                var recognized = promptValidatorContext.Recognized.Value.First();

                DateTime selectedDate = Convert.ToDateTime(recognized.Value);

                TimeSpan start = new TimeSpan(9,0,0);
                TimeSpan end = new TimeSpan(17,0,0);

                if(selectedDate.TimeOfDay >= start && selectedDate.TimeOfDay <= end)
                {
                    valid = true;
                }
            }

            return Task.FromResult(valid);

        }

        private Task<bool> PhoneNumberValidatorAsync(PromptValidatorContext<string> promptValidatorContext, CancellationToken cancellationToken)
        {

            var valid = false;

            if (promptValidatorContext.Recognized.Succeeded)
            {
                var recognized = promptValidatorContext.Recognized.Value;

                valid = Regex.Match(recognized, "^((\\(?\\d{3}\\)? \\d{4})|(\\(?\\d{4}\\)? \\d{3})|(\\(?\\d{5}\\)? \\d{2}))-\\d{4}$").Success;
            }

            return Task.FromResult(valid);

        }
    }
}
