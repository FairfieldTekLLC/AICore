//using System.ComponentModel;
//using System.Diagnostics;
//using Microsoft.SemanticKernel;
//using SematicKernelWeb.Domain;
//using SematicKernelWeb.Domain.Data_Classes;
//using SematicKernelWeb.Services;

//namespace SematicKernelWeb.SemanticKernel.Extensions;

//public class InternetUrlLoadPlugin
//{
//    private readonly IServiceScopeFactory _serviceScopeFactory;

//    public InternetUrlLoadPlugin(IBackendWorker backend, IServiceScopeFactory scopeFactory)
//    {
//        _backend = backend;
//        _serviceScopeFactory = scopeFactory;
//    }

//    private IBackendWorker _backend { get; }


//    [KernelFunction("load-the-url")]
//    [Description("Loads the passed url into the long term memory.")]
//    public async Task<string> LoadTheUrl(
//        [Description("The conversation Id")] Guid conversationId,
//        [Description("Owner Id")] Guid ownerId,
//        [Description("url")] string url)
//    {
//        Conversation conversation;
//        Guid conversationIdGuid = conversationId;

//        if (conversationIdGuid == Guid.Empty) return "";
//        conversation = new Conversation();
//        conversation.Load(conversationIdGuid);

//        Entry entry = new Entry
//        {
//            Type = ConversationType.WebSearch,
//            Text = url,
//            NumberOfResults = Conversation.MaxNumberOfResults,
//            Role = Role.user,
//            Sequence = conversation.PromptsOrSearches.Count + 1,
//            FetchedDocuments = new List<HTMLDocs>()
//        };
//        _backend.getLogger()
//            .LogInformation(
//                "Internet Url Load Plugin called with url: \"{Url}\" Conversation Id : {ConversationId} Owner Id: {OwnerId}",
//                url, conversationId, ownerId);

//        _backend.SendMessage(conversationId, "(Plugin) Started Processing url: " + url).ConfigureAwait(true)
//            .GetAwaiter().GetResult();

//        Debug.WriteLine("Processing url: " + url);
//        try
//        {
//            HTMLDocs doc = new HTMLDocs
//            {
//                Uri = url,
//                Body = "",
//                Summary = "",
//                MemoryKey = _backend.getISemanticKernelService().ImportWebPage(url, conversationId, ownerId).Result
//            };
//            entry.FetchedDocuments.Add(doc);
//        }
//        catch (Exception e)
//        {
//            _backend.getLogger().LogError(e, "Error processing url: {Url} for conversation Id: {ConversationId}", url,
//                conversationId);
//        }


//        Debug.WriteLine("Finished loading file! " + url);
//        _backend.SendMessage(conversationId, "(Plugin) Finished Processing url: " + url).ConfigureAwait(true)
//            .GetAwaiter().GetResult();

//        _backend.getLogger().LogInformation("Internet Url Load Plugin completed for conversation Id: {ConversationId}",
//            conversationId);
//        return "ok";
//    }
//}

