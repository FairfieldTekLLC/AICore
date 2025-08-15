namespace AICore.Classes;

public class Result
{
    public string Url { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string Thumbnail { get; set; }
    public string Engine { get; set; }
    public string Template { get; set; }
    public List<string> ParsedUrl { get; set; }
    public string ImgSrc { get; set; }
    public string Priority { get; set; }
    public List<string> Engines { get; set; }
    public List<int> Positions { get; set; }
    public double Score { get; set; }
    public string Category { get; set; }
    public object PublishedDate { get; set; }
}

public class SearXngResult
{
    public string Query { get; set; }
    public int NumberOfResults { get; set; }
    public List<Result> Results { get; set; }
    public List<object> Answers { get; set; }
    public List<object> Corrections { get; set; }
    public List<object> Infoboxes { get; set; }
    public List<string> Suggestions { get; set; }
    public List<List<string>> UnresponsiveEngines { get; set; }
}