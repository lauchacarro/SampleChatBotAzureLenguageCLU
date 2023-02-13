using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder.Dialogs;
using PluralsightBot.Services;
using Microsoft.Extensions.Logging;
using PluralsightBot.Helpers;

namespace PluralsightBot.Bots
{
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        private readonly BotStateService _stateService;
        private readonly Dialog _dialog;
        private readonly ILogger<DialogBot<T>> _logger;

        public DialogBot(BotStateService stateService, T dialog, ILogger<DialogBot<T>> logger)
        {
            _stateService = stateService;
            _dialog = dialog;
            _logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await _stateService.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _stateService.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running Dialog from Message Activity");


            await _dialog.Run(turnContext, _stateService.DialogStateAccessor, cancellationToken);

        }
    }
}
