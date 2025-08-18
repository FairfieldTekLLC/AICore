using AICore.Classes;
using AICore.Service;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using System.Text;

namespace AICore.SemanticKernel.Extensions;

public class InternetUrlLoadPlugin
{
    private readonly ISemanticKernelService _kernal;
    private readonly IBackend _backend;

    public InternetUrlLoadPlugin(ISemanticKernelService kernal, IBackend backend)
    {
        _kernal = kernal;
        _backend = backend;
        
    }


    [KernelFunction("load-the-url")]
    [Description("Load the url")]
    public async Task<string> LoadTheUrl(
        [Description("The conversation Id")] Guid conversationId,
        [Description("Owner Id")] Guid ownerId,
        [Description("url")] string url)
    {
        _backend.SendMessage(conversationId,"Attempting to load the url '" + url + "'");
        ChatMessageContentItemCollection col = new ChatMessageContentItemCollection();
        string key = "";
        try
        {
            key = _kernal.ImportWebPage(url, conversationId, ownerId).Result;
            _backend.SendMessage(conversationId, "Loaded the url '" + url + "'");
        }
        catch (Exception e)
        {
            _backend.SendMessage(conversationId, "Failed to loaded the url '" + url + "'");
        }


        if (!string.IsNullOrEmpty(key))
#pragma warning disable SKEXP0110
            col.Add(new FileReferenceContent(key));
#pragma warning restore SKEXP0110
        ChatHistory _hist = StaticHelpers.GetChatHistory(conversationId);
        _hist.AddMessage(AuthorRole.System, col, Encoding.ASCII, new Dictionary<string, object>());
        StaticHelpers.SaveChatHistory(conversationId, _hist);
        return "ok";
    }
}