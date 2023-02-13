using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using PluralsightBot.Services;
using PluralsightBot.Models;

namespace PluralsightBot.Bots
{
    public class GreetingBot : ActivityHandler
    {

        private readonly BotStateService _stateService;

        public GreetingBot(BotStateService stateService)
        {
            _stateService = stateService;
        }

        private async Task GetNameAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(turnContext, () => new());
            ConversationData conversationData = await _stateService.ConversationDataAccessor.GetAsync(turnContext, () => new());

            if (!string.IsNullOrWhiteSpace(userProfile.Name))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(string.Format(format: "Hi {0}, How can i help you today?", userProfile.Name)), cancellationToken);

            }
            else
            {
                if (conversationData.PropmtUserForName)
                {
                    userProfile.Name = turnContext.Activity.Text.Trim();

                    await turnContext.SendActivityAsync(MessageFactory.Text(string.Format("Tranks {0}, How can i help you today?", userProfile.Name)), cancellationToken);

                    conversationData.PropmtUserForName = false;
                }
                else
                {

                    await turnContext.SendActivityAsync(MessageFactory.Text("Whats is your name?"), cancellationToken);

                    conversationData.PropmtUserForName = true;
                }

                await _stateService.UserProfileAccessor.SetAsync(turnContext, userProfile);
                await _stateService.ConversationDataAccessor.SetAsync(turnContext, conversationData);

                await _stateService.UserState.SaveChangesAsync(turnContext);
                await _stateService.ConversationState.SaveChangesAsync(turnContext);
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await GetNameAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await GetNameAsync(turnContext, cancellationToken);
                }
            }
        }
    }
}
