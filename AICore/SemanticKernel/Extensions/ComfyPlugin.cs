using System.ComponentModel;
using System.Net;
using AICore.Classes;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AICore.SemanticKernel.Extensions;

public class ComfyPlugin
{
    private readonly IServiceScopeFactory _serviceScopeFactory;


    public ComfyPlugin(IServiceScopeFactory scopeFactory)
    {
        _serviceScopeFactory = scopeFactory;
    }


    private string GeneratePrompt(string text, string fileName)
    {
        return @"
        {
            ""3"": {
                ""class_type"": ""KSampler"",
                ""inputs"": {
                    ""cfg"": 8,
                    ""denoise"": 1,
                    ""latent_image"": [
                        ""5"",
                        0
                    ],
                    ""model"": [
                        ""4"",
                        0
                    ],
                    ""negative"": [
                        ""7"",
                        0
                    ],
                    ""positive"": [
                        ""6"",
                        0
                    ],
                    ""sampler_name"": ""euler"",
                    ""scheduler"": ""normal"",
                    ""seed"": 8566257,
                    ""steps"": 20
                }
            },
            ""4"": {
                ""class_type"": ""CheckpointLoaderSimple"",
                ""inputs"": {
                    ""ckpt_name"": ""v1-5-pruned-emaonly.safetensors""
                }
            },
            ""5"": {
                ""class_type"": ""EmptyLatentImage"",
                ""inputs"": {
                    ""batch_size"": 1,
                    ""height"": 512,
                    ""width"": 512
                }
            },
            ""6"": {
                ""class_type"": ""CLIPTextEncode"",
                ""inputs"": {
                    ""clip"": [
                        ""4"",
                        1
                    ],
                    ""text"": """ + text + @"""
                }
            },
            ""7"": {
                ""class_type"": ""CLIPTextEncode"",
                ""inputs"": {
                    ""clip"": [
                        ""4"",
                        1
                    ],
                    ""text"": ""bad hands""
                }
            },
            ""8"": {
                ""class_type"": ""VAEDecode"",
                ""inputs"": {
                    ""samples"": [
                        ""3"",
                        0
                    ],
                    ""vae"": [
                        ""4"",
                        2
                    ]
                }
            },
            ""9"": {
                ""class_type"": ""SaveImage"",
                ""inputs"": {
                    ""filename_prefix"": """ + fileName + @""",
                    ""images"": [
                        ""8"",
                        0
                    ]
                }
            }
        }";
    }


    private void QueuePrompt(string prompt)
    {
        try
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Config.Instance.ComfyUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write("{\"prompt\": " + prompt + "}");
            }

            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                string result = streamReader.ReadToEnd();
                Console.WriteLine(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }


    [KernelFunction("draw-picture")]
    [Description("draw me a picture of")]
    public async Task<string> Work(
        [Description("The conversation Id")] Guid conversationId,
        [Description("Owner Id")] Guid ownerId,
        [Description("of")] string imageDescription)
    {
        Guid g = Guid.NewGuid();

        string prompt = GeneratePrompt(imageDescription, g.ToString());

        QueuePrompt(prompt);

        ChatHistory _hist = StaticHelpers.GetChatHistory(conversationId);


        string fileName = Config.Instance.ComfyOutPutFolder + g + "_00001_.png";


        int counter = 0;
        while (!File.Exists(fileName))
        {
            Thread.Sleep(1000);
            counter++;
            if (counter > 60) // wait for 60 seconds max
                return "timeout";
        }

        byte[] data = null;
        counter = 0;
        if (File.Exists(fileName))
            while (true)
            {
                counter++;

                try
                {
                    data = File.ReadAllBytes(fileName);
                    File.Delete(fileName);
                    break;
                }
                catch (Exception e)
                {
                    Thread.Sleep(1000);
                }

                if (counter > 60) // wait for 60 seconds max
                    return "timeout";
            }

        if (data == null)
            return "oops";


        string mimeType = fileName.GetMimeTypeForFileExtension();
        ChatMessageContentItemCollection col = new ChatMessageContentItemCollection
        {
            new TextContent(imageDescription, Config.Instance.Model),
            new ImageContent(new ReadOnlyMemory<byte>(data), mimeType)
        };
        _hist.AddMessage(AuthorRole.Assistant, col);
        StaticHelpers.SaveChatHistory(conversationId, _hist);


        //return "<img style='width:100px;height:100px' src='data:" + mimeType +
        //    ";base64," +
        //    Convert.ToBase64String(data) + "' />";

        return "ok";
    }
}