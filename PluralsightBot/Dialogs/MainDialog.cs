using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using PluralsightBot.Models;
using PluralsightBot.Services;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Text.RegularExpressions;

namespace PluralsightBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {

        private readonly BotStateService _stateService;

        public MainDialog(BotStateService stateService) : base(nameof(MainDialog))
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

            AddDialog(new GreetingDialog($"{nameof(MainDialog)}.greeting", _stateService));
            AddDialog(new BugReportDialog($"{nameof(MainDialog)}.bugreport", _stateService));
            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainflow", waterfallSteps));

            InitialDialogId = $"{nameof(MainDialog)}.mainflow";
        }

        private async Task<DialogTurnResult> InitialStepDialog(WaterfallStepContext waterfallStepContext, CancellationToken cancellationToken)
        {
            if(Regex.Match(waterfallStepContext.Context.Activity.Text.Trim().ToLower(), "hi").Success)
            {
                return await waterfallStepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", null, cancellationToken);
            }
            else
            {
                return await waterfallStepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugreport", null, cancellationToken);

            }

        }

        private async Task<DialogTurnResult> FinalStepDialog(WaterfallStepContext waterfallStepContext, CancellationToken cancellationToken)
        {
            return await waterfallStepContext.EndDialogAsync(null, cancellationToken);

        }
    }
}
