using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

using PluralsightBot.Models;

namespace PluralsightBot.Services
{
    public class BotStateService
    {
        public BotStateService(UserState userState, ConversationState conversationState)
        {
            UserState = userState;
            ConversationState = conversationState;
            InitializeAccessor();
        }

        public ConversationState ConversationState { get; }
        public UserState UserState { get; }

        public static string UserProfileId { get; } = $"{nameof(BotStateService)}.UserProfile";
        public static string ConversationDataId { get; } = $"{nameof(BotStateService)}.ConversationData";
        public static string DialogStateId { get; } = $"{nameof(BotStateService)}.DialogState";

        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }
        public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        private void InitializeAccessor()
        {
            UserProfileAccessor = UserState.CreateProperty<UserProfile>(UserProfileId);
            ConversationDataAccessor = ConversationState.CreateProperty<ConversationData>(ConversationDataId);
            DialogStateAccessor = ConversationState.CreateProperty<DialogState>(DialogStateId);
        }
    }
}
