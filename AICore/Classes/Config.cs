using Newtonsoft.Json;

namespace AICore.Classes;

public class Config
{
    private static readonly Lazy<Config> Lazy = new(() => new Config());

    private Config()
    {
    }

    public static Config Instance => Lazy.Value;

    public string ComfyUrl { get; set; }

    public int NumberOfResultsForInternetSearch { get; set; }
    public string ComfyOutPutFolder { get; set; }

    public string EmbeddingModel { get; set; }

    public string Model { get; set; }

    public string OllamaServerUrl { get; set; }

    public string SearXngUrl { get; set; }

    public List<string> IgnoreSites { get; set; }
    public string SystemPrompt { get; set; }
    public string VisionModel { get; set; }


    public string DatabaseName { get; set; }
    public string ConnectionString { get; set; }
    public string LogFilePath { get; set; }
    public LogLevel CurrentLogLevel { get; set; }

    public void Load()
    {
        string json;
        if (File.Exists("config-Debug.json"))
            json = File.ReadAllText("config-Debug.json");

        else if (File.Exists("config.json"))
            json = File.ReadAllText("config.json");
        else
            throw new Exception("No Configuration");
        Config? config = JsonConvert.DeserializeObject<Config>(json);
        if (config != null)
        {
            ComfyOutPutFolder = config.ComfyOutPutFolder;
            EmbeddingModel = config.EmbeddingModel;
            Model = config.Model;
            OllamaServerUrl = config.OllamaServerUrl;
            SearXngUrl = config.SearXngUrl;
            IgnoreSites = config.IgnoreSites;
            SystemPrompt = config.SystemPrompt;
            DatabaseName = config.DatabaseName;
            ConnectionString = config.ConnectionString;
            LogFilePath = config.LogFilePath;
            CurrentLogLevel = config.CurrentLogLevel;
            ComfyUrl = config.ComfyUrl;
            VisionModel = config.VisionModel;
            NumberOfResultsForInternetSearch = config.NumberOfResultsForInternetSearch;
        }
    }
}