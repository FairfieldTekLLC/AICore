using System.ComponentModel.DataAnnotations;
using AICore.Controllers.ViewModels;
using AICore.Hubs;
using AICore.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp.Models.Chat;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AICore.Classes;

public static class StaticHelpers
{
    public static string GetMimeTypeForFileExtension(this string filePath)
    {
        const string defaultContentType = "application/octet-stream";

        FileExtensionContentTypeProvider provider = new FileExtensionContentTypeProvider();

        if (!provider.TryGetContentType(filePath, out string contentType)) contentType = defaultContentType;

        return contentType;
    }

    public static string SerializeThis<type>(this object obj)
    {
        string jsonString = JsonSerializer.Serialize(obj);
        return jsonString;
    }

    public static object DeserializeThis<type>(this string json)
    {
        type? doh = JsonSerializer.Deserialize<type>(json);
        return doh;
    }

    public static void SaveChatHistory(Guid conversationID, ChatHistory hist)
    {
        using NewsReaderContext ctx = new NewsReaderContext();
        Conversation? obj = ctx.Conversations.FirstOrDefault(x => x.Pkconversationid == conversationID);
        obj.Serializedchat = hist.SerializeThis<ChatHistory>();
        ctx.SaveChanges();
    }

    public static ChatHistory GetChatHistory(Guid conversationId)
    {
        ChatHistory hist;
        using NewsReaderContext ctx = new NewsReaderContext();
        Conversation? obj = ctx.Conversations.FirstOrDefault(x => x.Pkconversationid == conversationId);
        if (string.IsNullOrEmpty(obj.Serializedchat))
        {
            hist = new ChatHistory();

            string prompt = "Owner Id: " + obj.Fksecurityobjectowner + "\r";
            prompt += "Conversation Id:" + conversationId + "\r";


            hist.AddSystemMessage(prompt);
            obj.Serializedchat = hist.SerializeThis<ChatHistory>();
            ctx.SaveChanges();
        }
        else
        {
            hist = (ChatHistory)obj.Serializedchat.DeserializeThis<ChatHistory>();
        }

        return hist;
    }

    public static string FirstCharToUpper(this string input)
    {
        return input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input[0].ToString().ToUpper() + input.Substring(1)
        };
    }

    public static async Task SendMessage(this IHubContext<ChatHub> _hubContext,Guid conversationId,string message)
    {
        await _hubContext.Clients.Groups(conversationId.ToString()).SendCoreAsync("ReceiveMessage", new[] { conversationId.ToString(),message });


    }
    

    public static bool IsValidEmailAddress(this string emailAddress)
    {
        var emailValidation = new EmailAddressAttribute();
        return emailValidation.IsValid(emailAddress);
    }

}