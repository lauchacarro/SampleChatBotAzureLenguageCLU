using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

using PluralsightBot.Models;
using PluralsightBot.Services;

using System.Threading;
using System.Threading.Tasks;

namespace PluralsightBot.Dialogs
{
    public class GreetingDialog : ComponentDialog
    {

        private readonly BotStateService _stateService;

        public GreetingDialog(string dialogId, BotStateService stateService) : base(dialogId)
        {
            _stateService = stateService;

            InitializeWaterfallDialog();
        }

        void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepDialog,
                FinalStepDialog
            };

            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainflow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.name"));

            InitialDialogId = $"{nameof(GreetingDialog)}.mainflow";
        }

        private async Task<DialogTurnResult> InitialStepDialog(WaterfallStepContext waterfallStepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(waterfallStepContext.Context, () => new(), cancellationToken);

            if (string.IsNullOrWhiteSpace(userProfile.Name))
            {
                return await waterfallStepContext.PromptAsync($"{nameof(GreetingDialog)}.name", new PromptOptions
                {
                    Prompt = MessageFactory.Text("Whats yout name?")
                }, cancellationToken);
            }
            else
            {
                return await waterfallStepContext.NextAsync(null, cancellationToken);
            }

        }

        private async Task<DialogTurnResult> FinalStepDialog(WaterfallStepContext waterfallStepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(waterfallStepContext.Context, () => new(), cancellationToken);

            if (string.IsNullOrWhiteSpace(userProfile.Name))
            {
                userProfile.Name = (string)waterfallStepContext.Result;

                await _stateService.UserProfileAccessor.SetAsync(waterfallStepContext.Context, userProfile, cancellationToken);


            }


            await waterfallStepContext.Context.SendActivityAsync(MessageFactory.Text(string.Format(format: "Hi {0}, How can i help you today?", userProfile.Name)), cancellationToken);

            return await waterfallStepContext.EndDialogAsync(null, cancellationToken);

        }
    }
}
