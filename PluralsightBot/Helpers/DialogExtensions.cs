using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

using System.Threading;
using System.Threading.Tasks;

namespace PluralsightBot.Helpers
{
    public static class DialogExtensions
    {
        public static async Task Run(this Dialog dialog, ITurnContext turnContext, IStatePropertyAccessor<DialogState> accessor, CancellationToken cancellationToken)
        {
            var dialogSet = new DialogSet(accessor);

            dialogSet.Add(dialog);

            var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);

            var dialogResult = await dialogContext.ContinueDialogAsync(cancellationToken);

            if(dialogResult.Status == DialogTurnStatus.Empty)
            {
                await dialogContext.BeginDialogAsync(dialog.Id, null, cancellationToken);
            }

        }
    }
}
