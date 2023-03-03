using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using PluralsightBot.Models;
using PluralsightBot.Services;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Text.RegularExpressions;
using Azure;
using Azure.Core;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using System.Linq;
using PluralsightBot.Helpers;

namespace PluralsightBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {

        private readonly BotStateService _stateService;
        private readonly BotService _botService;

        public MainDialog(BotStateService stateService, BotService botService) : base(nameof(MainDialog))
        {
            _stateService = stateService;

            InitializeWaterfallDialog();
            _botService = botService;
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

            string projectName = "PS-CustomerService";
            string deploymentName = "1.0";
            string participantId = waterfallStepContext.Context.Activity.From.Id;

            var data = new
            {
                analysisInput = new
                {
                    conversationItem = new
                    {
                        text = waterfallStepContext.Context.Activity.Text,
                        id = participantId,
                        participantId,
                    }
                },
                parameters = new
                {
                    projectName,
                    deploymentName,

                    // Use Utf16CodeUnit for strings in .NET.
                    stringIndexType = "Utf16CodeUnit",
                },
                kind = "Conversation",
            };

            Response response = await _botService.Client.AnalyzeConversationAsync(RequestContent.Create(data));

            using JsonDocument result = JsonDocument.Parse(response.ContentStream);

            var cluModel = result.Deserialize<CluModel>(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var entities = cluModel.Result.Prediction.Entities.OrderByDescending(x => x.Offset);


            switch (cluModel.Result.Prediction.TopIntent)
            {
                case "GreetingIntent":
                    return await waterfallStepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", null, cancellationToken);


                case "NewBugReportIntent":

                    var phoneNumber = entities.FirstOrDefault(x => x.Category == "BugReport.PhoneNumber")?.Text;
                    var bug = entities.FirstOrDefault(x => x.Category == "BugReport.Description.Bug")?.ExtraInformation.First().Key;
                    var description = entities.FirstOrDefault(x => x.Category == "BugReport.Description")?.Text;
                    var callbackTime = entities.FirstOrDefault(x => x.Category == "BugReport.CallbackTime")?.Text;

                    UserProfile userProfile = new()
                    {
                        PhoneNumber = phoneNumber,
                        Bug = bug,
                        Description = description,
                        CallbackTime = string.IsNullOrWhiteSpace(callbackTime) ? null : AIRecogniser.RecogniseDateTime(callbackTime, out _)

                    };


                    return await waterfallStepContext.BeginDialogAsync($"{nameof(MainDialog)}.bugreport", userProfile, cancellationToken);


                default:
                    await waterfallStepContext.Context.SendActivityAsync(MessageFactory.Text("No se encontro un dialogo."), cancellationToken);
                    break;
            }

            return await waterfallStepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepDialog(WaterfallStepContext waterfallStepContext, CancellationToken cancellationToken)
        {
            return await waterfallStepContext.EndDialogAsync(null, cancellationToken);

        }
    }
}
