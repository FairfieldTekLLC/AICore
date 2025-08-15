using System.Diagnostics.CodeAnalysis;
using AICore.Classes;
using AICore.Controllers.ViewModels;
using AICore.Models;
using AICore.SemanticKernel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AICore.Controllers;

public class ConversationController(ILogger<HomeController> logger, ISemanticKernelService semanticKernelService)
    : BaseController(logger, semanticKernelService)
{
    public IActionResult CreateConversation(string title, string description)
    {
        using (NewsReaderContext ctx = new NewsReaderContext())
        {
            Conversation conversation = new Conversation
            {
                Pkconversationid = Guid.NewGuid(),
                Title = title,
                Description = description,
                Fksecurityobjectowner = GetOwnerId(),
                Createdat = DateTime.Now,
                Serializedchat = ""
            };
            ctx.Conversations.Add(conversation);
            ctx.SaveChanges();
        }

        return Json(new
        {
            result = "Created Conversation",
            success = true
        });
    }

    [Experimental("SKEXP0110")]
    public IActionResult DeleteConversation(Guid conversationId)
    {
        if (conversationId == Guid.Empty)
            return Json(new
            {
                result = "Invalid Conversation ID",
                success = false
            });
        using (NewsReaderContext ctx = new NewsReaderContext())
        {
            Conversation? conversation = ctx.Conversations.FirstOrDefault(x => x.Pkconversationid == conversationId);
            if (conversation == null)
                return Json(new
                {
                    result = "Conversation not found",
                    success = false
                });

            ChatHistory hist = StaticHelpers.GetChatHistory(conversationId);
            foreach (ChatMessageContent content in hist)
            foreach (KernelContent contentItem in content.Items)
                switch (contentItem)
                {
                    case FileReferenceContent fileContent:
                        if (!string.IsNullOrEmpty(fileContent.FileId))
                            _semanticKernelService.RemoveFile(fileContent.FileId, GetOwnerId()).Wait();

                        break;
                }

            ctx.Conversations.Remove(conversation);
            ctx.SaveChanges();
            return Json(new
            {
                result = "Deleted Conversation",
                success = true
            });
        }
    }

    public IActionResult CreateConversationView()
    {
        return PartialView();
    }

    public IActionResult ConversationList()
    {
        List<ConversationVm> conversations = new List<ConversationVm>();

        using (NewsReaderContext ctx = new NewsReaderContext())
        {
            List<Conversation> convs = ctx.Conversations.Where(x => x.Fksecurityobjectowner == GetOwnerId())
                .ToList();
            conversations.AddRange(convs.Select(conversation => new ConversationVm
            {
                ParentId = conversation.Fkparentid, Title = conversation.Title, Description = conversation.Description,
                Id = conversation.Pkconversationid
            }));
        }

        return View(conversations);
    }
}