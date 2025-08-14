using AICore.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using System.Text;
using System.Text.Json;

namespace AICore.Classes;

public static class StaticHelpers
{
    public static string GetMimeTypeForFileExtension(this string filePath)
    {
        const string defaultContentType = "application/octet-stream";

        var provider = new FileExtensionContentTypeProvider();

        if (!provider.TryGetContentType(filePath, out string contentType)) contentType = defaultContentType;

        return contentType;
    }

    static JsonSerializerSettings settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
    public static string SerializeThis<type>(this object obj)
    {
        string jsonString = System.Text.Json.JsonSerializer.Serialize(obj);
        return jsonString;

        var json = JsonConvert.SerializeObject(obj, typeof(type), settings);
        return json;
    }

    public static object DeserializeThis<type>(this string json)
    {
        var doh = System.Text.Json.JsonSerializer.Deserialize<type>(json);
        return doh;
        var deserializedObj = JsonConvert.DeserializeObject<type>(json, settings);
        return deserializedObj;
    }

    public static void SaveChatHistory(Guid conversationID, ChatHistory hist)
    {
        using (var ctx = new NewsReaderContext())
        {
            var obj = ctx.Conversations.FirstOrDefault(x => x.Pkconversationid == conversationID);
            obj.SerializedChat = hist.SerializeThis<ChatHistory>();
            ctx.SaveChanges();
        }
    }
    public static ChatHistory GetChatHistory(Guid conversationId)
    {
        ChatHistory hist;
        using (var ctx = new NewsReaderContext())
        {
            var obj = ctx.Conversations.FirstOrDefault(x => x.Pkconversationid == conversationId);
            if (string.IsNullOrEmpty(obj.SerializedChat))
            {
                hist = new ChatHistory();

                string prompt = "Owner Id: " + obj.Fksecurityobjectowner + "\r";
                prompt += "Conversation Id:" + conversationId + "\r";


                hist.AddSystemMessage(prompt);
                obj.SerializedChat = hist.SerializeThis<ChatHistory>();
                ctx.SaveChanges();
            }
            else
            {
                hist = (ChatHistory)obj.SerializedChat.DeserializeThis<ChatHistory>();

            }
        }

        return hist;
    }

}