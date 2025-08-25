using AICore.Classes;
using AICore.Service;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;

namespace AICore.SemanticKernel.Extensions
{
    public class TextToSpeakPlugin(ISemanticKernelService kernal, IBackend backend)
    {
        private readonly ISemanticKernelService _kernal = kernal;
        private readonly IBackend _backend = backend;


        public async Task<string> SayIt(Guid conversationId, Guid ownerId, string? textToSay)
        {
            //_backend.SendMessage(conversationId, "So you want me to draw you a picture of '" + imageDescription + "'?");
            //Generate a unique identifier for the image
            Guid g = Guid.NewGuid();

            _backend.SendMessage(conversationId, "Generating prompt");
            //Load the chat history
            ChatHistory _hist = StaticHelpers.GetChatHistory(conversationId);


            string mimeType = ".mp3".GetMimeTypeForFileExtension();
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            ChatMessageContentItemCollection col = new ChatMessageContentItemCollection
        {
            new TextContent(textToSay, Config.Instance.Model),
            new AudioContent(textToSay.ToSpeech(),mimeType)
        };
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            _hist.AddMessage(AuthorRole.Assistant, col);
            StaticHelpers.SaveChatHistory(conversationId, _hist);

            return "ok";
        }




        [KernelFunction("tts1")]
        [Description("audio Speak this to me")]
        public async Task<string> TextToSpeech(
            [Description("The conversation Id")] Guid conversationId,
            [Description("Owner Id")] Guid ownerId,
            [Description("what")] string whatToSay)
        {
            return await SayIt(conversationId, ownerId, whatToSay);
        }

        [KernelFunction("tts2")]
        [Description("audio Say this to me")]
        public async Task<string> TextToSpeech1(
            [Description("The conversation Id")] Guid conversationId,
            [Description("Owner Id")] Guid ownerId,
            [Description("what")] string whatToSay)
        {
            return await SayIt(conversationId, ownerId, whatToSay);
        }

        [KernelFunction("tts3")]
        [Description("Read it to me ")]
        public async Task<string> TextToSpeech2(
            [Description("The conversation Id")] Guid conversationId,
            [Description("Owner Id")] Guid ownerId,
            [Description("what")] string whatToSay)
        {
            return await SayIt(conversationId, ownerId, whatToSay);
        }

    }
}
