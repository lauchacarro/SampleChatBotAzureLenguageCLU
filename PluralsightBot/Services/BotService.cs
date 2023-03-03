using Azure;
using Azure.AI.Language.Conversations;
using Azure.Identity;

using System;

namespace PluralsightBot.Services
{
    public class BotService
    {
        public BotService()
        {
            Uri endpoint = new Uri("https://virtual-assistant-openai-demo.cognitiveservices.azure.com");


            Client = new ConversationAnalysisClient(endpoint, new AzureKeyCredential("b9596325af284a8b973a963fbb44a5de"));
        }

        public ConversationAnalysisClient Client { get; }
    }
}
