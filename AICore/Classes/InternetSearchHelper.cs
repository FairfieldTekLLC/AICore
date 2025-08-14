using AICore.Domain.Data_Classes;
using HtmlAgilityPack;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using PuppeteerSharp;

namespace AICore.Classes;

public static class InternetSearchHelper
{
    public static async Task<string> DownloadWebpageAsync(this string url)
    {
        using HttpClient client = new HttpClient();
        return await client.GetStringAsync(url);
    }

    public static List<Result> QuerySearchEngineForUrls(this List<string> browserSearchTerms, int numberOfResults)
    {
        //List<string> urls = new List<string>();
        List<Result> urls = new List<Result>();

        foreach (string term in browserSearchTerms)
        {
            string url = Config.Instance.SearXngUrl + term;
            string result = url.DownloadWebpageAsync().GetAwaiter().GetResult();
            SearXngResult? resultObj = JsonConvert.DeserializeObject<SearXngResult>(result);
            if (resultObj == null)
            {
                Console.WriteLine("No results found for term: " + term);
                continue;
            }

            urls = new List<Result>(resultObj.Results.Select(r => r));
        }


        return urls.Take(numberOfResults).ToList();
    }

    public static async Task<byte[]> FetchUrlAsPdf(this string url)
    {
        try
        {
            BrowserFetcher browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using IBrowser? browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using IPage? page = await browser.NewPageAsync();
            await page.SetCacheEnabledAsync(false);
            await page.SetJavaScriptEnabledAsync(true);
            await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);

            byte[]? doc = await page.PdfDataAsync();
            return doc;
        }
        catch (Exception e)
        {
            return new List<byte>().ToArray();
        }
    }

    public static string ReadPDf(byte[] pdfbytes)
    {
        var reader = new PdfReader(pdfbytes);

        var stringsList = new List<string>();
        for (var pageNum = 1; pageNum <= reader.NumberOfPages; pageNum++)
        {
            // Get the page content and tokenize it.
            var contentBytes = reader.GetPageContent(pageNum);
            var tokenizer = new PrTokeniser(new RandomAccessFileOrArray(contentBytes));


            while (tokenizer.NextToken())
                if (tokenizer.TokenType == PrTokeniser.TK_STRING)
                    // Extract string tokens.
                    stringsList.Add(tokenizer.StringValue);

            // Print the set of string tokens, one on each line.
        }

        Console.WriteLine(string.Join("\r\n", stringsList));

        reader.Close();
        return string.Join("\r\n", stringsList);
    }


    public static async Task<string> FetchUrlAsContent(this string url)
    {
        try
        {
            try
            {
                BrowserFetcher browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                await using IBrowser? browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                await using IPage? page = await browser.NewPageAsync();
                await page.SetCacheEnabledAsync(false);
                await page.SetJavaScriptEnabledAsync(true);
                await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);

                byte[]? doc = await page.PdfDataAsync();
                return StripShit(ReadPDf(doc));
            }
            catch (Exception e)
            {
                return "";
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "";
        }
    }

    public static string ConvertHtmlToPlainText(this string html)
    {
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);
        return htmlDoc.DocumentNode.InnerText.Trim();
    }

    public static string StripShit(string shit)
    {
        return RemoveNonPrintableCharacters(shit);
    }

    public static string RemoveNonPrintableCharacters(string input)
    {
        // Filter out characters that are control characters
        // Control characters are generally non-printable and used for formatting
        return new string(input.Where(c => !char.IsControl(c)).ToArray());
    }
}