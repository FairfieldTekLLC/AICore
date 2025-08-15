using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using AICore.Classes;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AICore.SemanticKernel.Extensions;

public class InternetSearchPlugin
{
    private readonly ISemanticKernelService _kernal;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public InternetSearchPlugin(IServiceScopeFactory scopeFactory, ISemanticKernelService kernal)
    {
        _serviceScopeFactory = scopeFactory;
        _kernal = kernal;
    }


    [KernelFunction("search_internet")]
    [Description("Search the internet for a subject.")]
    public async Task<string> SearchTheInternet(
        [Description("The conversation Id")] Guid conversationId,
        [Description("Owner Id")] Guid ownerId,
        [Description("for")] string searchString)
    {
        try
        {
            string content = await ImportWebSearch(conversationId, ownerId, searchString);
            return content;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "";
        }
    }


    private bool LoadSite(string url)
    {
        return Config.Instance.IgnoreSites.All(site =>
            !url.Contains(site, StringComparison.InvariantCultureIgnoreCase));
    }


    public async Task<string> ImportWebSearch(Guid conversationId, Guid ownerId, string passedSearchString)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();


        StringBuilder resultText = new StringBuilder();

        string searchString = passedSearchString ?? "";
        if (searchString.Contains(" today ", StringComparison.InvariantCultureIgnoreCase))
            searchString = searchString.Replace(" today ", DateTime.Now.ToString("D"));
        if (searchString.Contains(" today?", StringComparison.InvariantCultureIgnoreCase))
            searchString = searchString.Replace(" today?", DateTime.Now.ToString("D"));
        if (searchString.Contains(" today.", StringComparison.InvariantCultureIgnoreCase))
            searchString = searchString.Replace(" today.", DateTime.Now.ToString("D"));
        if (searchString.Contains(" today!", StringComparison.InvariantCultureIgnoreCase))
            searchString = searchString.Replace(" today!", DateTime.Now.ToString("D"));
        if (searchString.Contains(" today", StringComparison.InvariantCultureIgnoreCase))
            searchString = searchString.Replace(" today", DateTime.Now.ToString("D"));

        if (searchString.Contains(" today's", StringComparison.InvariantCultureIgnoreCase))
            searchString = searchString.Replace(" today's", DateTime.Now.ToString("D"));


        List<Result> SearchResults =
            new List<string> { searchString }.QuerySearchEngineForUrls(30);

        List<string> urls = new List<string>();


        int counter = 0;
        foreach (Result result in SearchResults)
        {
            if (LoadSite(result.Url))
            {
                urls.Add(result.Url);
                resultText.AppendLine("Title: " + result.Title);
                resultText.AppendLine("Url: <a href='" + result.Url + "' target='newWindow'>" + result.Url + "</a>");
                resultText.AppendLine("Content: " + result.Content);
                resultText.AppendLine("--------------------------------------------------");
                counter++;
            }

            if (counter > 5)
                break;
        }

        ChatMessageContentItemCollection col = new ChatMessageContentItemCollection();
        Parallel.ForEach(urls, new ParallelOptions { MaxDegreeOfParallelism = 8 }, url =>
        {
            try
            {
                Debug.WriteLine("Started Loading URL: " + url);
                string key = _kernal.ImportWebPage(url, conversationId, ownerId).Result;
                Debug.WriteLine("Finished Loading URL: " + url);
                if (!string.IsNullOrEmpty(key))
#pragma warning disable SKEXP0110
                    col.Add(new FileReferenceContent(key));
#pragma warning restore SKEXP0110
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error! " + e);
            }
        });

        ChatHistory _hist = StaticHelpers.GetChatHistory(conversationId);
        _hist.AddMessage(AuthorRole.System, col, Encoding.ASCII, new Dictionary<string, object>());
        _hist.AddAssistantMessage(resultText.ToString());
        StaticHelpers.SaveChatHistory(conversationId, _hist);
        return resultText.ToString();
    }
}