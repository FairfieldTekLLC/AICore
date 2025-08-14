using AICore.Controllers.ViewModels;
using AICore.Models;
using AICore.SemanticKernel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                    foreach (Entry entry in conversation.Entries)
                        if (entry.Fetcheddocs != null && entry.Fetcheddocs.Count > 0)
                            foreach (Fetcheddoc? doc in entry.Fetcheddocs)
                                if (!string.IsNullOrEmpty(doc.Memorykey))
                                    _semanticKernelService.RemoveFile(doc.Memorykey, GetOwnerId()).Wait();

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
