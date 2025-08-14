using System.Diagnostics.CodeAnalysis;
using AICore.Classes;
using AICore.Controllers.ViewModels;
using AICore.Models;
using AICore.SemanticKernel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AICore.Controllers
{
    public class ConversationController : BaseController
    {
        public ConversationController(ILogger<HomeController> logger, ISemanticKernelService semanticKernelService) : base(logger, semanticKernelService)
        {
        }

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
                    Createdat = DateTime.Now
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
                Conversation? conversation = ctx.Conversations.Include(x => x.Entries)
                    .ThenInclude(x => x.Fetcheddocs)
                    .FirstOrDefault(x => x.Pkconversationid == conversationId);
                if (conversation != null)
                {
                    ChatHistory hist = StaticHelpers.GetChatHistory(conversationId);
                    foreach (ChatMessageContent content in hist)
                    {
                        foreach (var contentItem in content.Items)
                        {
                            switch (contentItem)
                            {
                                case FileReferenceContent fileContent:
                                    if (!string.IsNullOrEmpty(fileContent.FileId))
                                    {
                                        _semanticKernelService.RemoveFile(fileContent.FileId, GetOwnerId()).Wait();
                                    }

                                    break;
                            }
                        }

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

            return Json(new
            {
                result = "Conversation not found",
                success = false
            });
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
                foreach (Conversation conversation in convs)
                    conversations.Add(new ConversationVm
                    {
                        ParentId = conversation.Fkparentid,
                        Title = conversation.Title,
                        Description = conversation.Description,
                        Id = conversation.Pkconversationid
                    });
            }

            return View(conversations);
        }
    }
}
