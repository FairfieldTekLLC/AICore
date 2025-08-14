using AICore.Classes;
using AICore.SemanticKernel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using System.Text;

namespace SematicKernelWeb.SemanticKernel.Extensions;

public class InternetUrlLoadPlugin
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private ISemanticKernelService _kernal;
    public InternetUrlLoadPlugin( IServiceScopeFactory scopeFactory, ISemanticKernelService kernal)
    {
        _kernal= kernal;
        _serviceScopeFactory = scopeFactory;
    }



    [KernelFunction("load-the-url")]
    [Description("Load the url")]
    public async Task<string> LoadTheUrl(
        [Description("The conversation Id")] Guid conversationId,
        [Description("Owner Id")] Guid ownerId,
        [Description("url")] string url)
    {
        var col = new ChatMessageContentItemCollection();
        string key = "";
        try
        {
             key= _kernal.ImportWebPage(url, conversationId, ownerId).Result;
        }
        catch (Exception e)
        {
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

