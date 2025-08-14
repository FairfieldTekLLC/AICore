using AICore.Classes;
using AICore.Controllers.ViewModels;
using AICore.Models;
using AICore.SemanticKernel;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace AICore.Controllers
{
    public class PromptController : BaseController
    {
        public PromptController(ILogger<HomeController> logger, ISemanticKernelService semanticKernelService) : base(logger, semanticKernelService)
        {
        }

        public IActionResult ProcessPrompt(Guid conversationId)
        {
            if (GetOwnerId() == Guid.Empty)
                Response.Redirect("/Home/LoginOrCreate");

            if (conversationId == Guid.Empty)
                Response.Redirect("/Home/LoginOrCreate");


            ConversationVm vm = new ConversationVm();
            using (NewsReaderContext ctx = new NewsReaderContext())
            {
                Models.Conversation? conversation =
                    ctx.Conversations.FirstOrDefault(x => x.Pkconversationid == conversationId);
                if (conversation != null)
                {
                    vm.Id = conversation.Pkconversationid;
                    vm.Title = conversation.Title;
                    vm.Description = conversation.Description;
                }
                else
                {
                    vm.Id = Guid.Empty;
                    vm.Title = "New Conversation";
                    vm.Description = "This is a new conversation. Please add prompts or searches.";
                }


                ChatHistory _hist = StaticHelpers.GetChatHistory(conversationId);

                vm.ConversationText = BuildOutput(_hist);
            }





            return View(vm);
        }
        public string BuildOutput(ChatHistory _hist)
        {
            bool first = true;
            string output = "<div class=\"accordion accordion-flush\" id=\"accordionFlushExample\">";
            int counter = 0;

            int lastcount = 0;
            foreach (ChatMessageContent content in _hist)
            {
                if (content.Role == AuthorRole.User || content.Role == AuthorRole.Assistant)
                    lastcount++;
            }



            foreach (ChatMessageContent content in _hist)
            {

                if ((content.Role == AuthorRole.User) || (content.Role == AuthorRole.Assistant))
                {
                    counter++;
                    string html = "<div>";
                    foreach (var contentItem in content.Items)
                    {
                        switch (contentItem)
                        {
                            case TextContent textContent:
                                html += "<div>" + textContent.Text + "</div>";
                                break;
                            case ImageContent imageContent:
                                html += "<div>" + "<img style='width:300px;height:300px' src='data:" +
                                        imageContent.MimeType +
                                        ";base64," +
                                        Convert.ToBase64String(imageContent.Data.Value.ToArray()) + "' />" + "</div>";
                                break;
                        }
                    }

                    html += "</div>";
                    output = MakeAccordian(content.Role.Label.FirstCharToUpper(), html, counter, lastcount == counter) + output;




                }
            }

            output += "</div>";
            return output;
        }
        public string MakeAccordian(string title, string body, int instance, bool show = false)
        {
            string output = @$"
    <div class=""accordion-item"">
    <h2 class=""accordion-header"">
      <button class=""accordion-button""  type=""button"" data-bs-toggle=""collapse"" data-bs-target=""#collapse{instance}"" aria-expanded=""true"" aria-controls=""collapse{instance}"">
        {title}
      </button>
    </h2>
    <div id=""collapse{instance}"" class=""accordion-collapse collapse {(show ? "show" : "")}"" data-bs-parent=""#accordionFlushExample"">
      <div class=""accordion-body"" style='text-align: left;'>
        {body.Replace("\n", "</br>")}
      </div>
    </div></div>
  ";

            return output;
        }

        [Experimental("SKEXP0110")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> ImageToText(List<IFormFile> uploadedFile, Guid conversationId,
    string txtpromptimgtotext)
        {
            ChatHistory _hist = StaticHelpers.GetChatHistory(conversationId);

            if (string.IsNullOrEmpty(txtpromptimgtotext))
                txtpromptimgtotext = "";

            if (uploadedFile == null || uploadedFile.Count == 0)
            {



                var col = new ChatMessageContentItemCollection
                {
                    new TextContent(txtpromptimgtotext)
                };
                var p = new Dictionary<string, object>
                {
                    { "Conversation Id", conversationId.ToString() },
                    { "Owner Id", GetOwnerId().ToString() }
                };
                _hist.AddMessage(AuthorRole.User, col, Encoding.ASCII, p);
            }
            else
            {
                foreach (IFormFile postedFile in uploadedFile)
                {
                    using Stream fs = postedFile.OpenReadStream();
                    byte[] byteArray = new byte[fs.Length];
                    fs.Read(byteArray, 0, (int)fs.Length);

                    string mimeType = postedFile.FileName.GetMimeTypeForFileExtension();

                    string ext = Path.GetExtension(postedFile.FileName).ToLowerInvariant();
                    ChatMessageContentItemCollection col;
                    switch (ext)
                    {
                        case ".jpg":
                        case ".jpeg":
                        case ".png":
                        case ".gif":
                        case ".bmp":
                        case ".webp":
                            col = new ChatMessageContentItemCollection
                        {
                            new TextContent(txtpromptimgtotext, Config.Instance.Model),
                            new ImageContent(new ReadOnlyMemory<byte>(byteArray), mimeType)
                        };
                            break;
                        case ".pdf":
                            string key = "";
                            using (Stream stream = new MemoryStream(byteArray))
                            {
                                key = await _semanticKernelService.ImportFile(postedFile.FileName, stream,
                                    conversationId, GetOwnerId());
                            }

                            col = new ChatMessageContentItemCollection
                        {
                            new TextContent(txtpromptimgtotext, Config.Instance.Model),
                            new FileReferenceContent(key)
                        };
                            break;
                        default:
                            return new ContentResult
                            {
                                Content = BuildOutput(_hist),
                                ContentType = "text/html"
                            };
                            break;
                    }
                    var p = new Dictionary<string, object>
                    {
                        { "Conversation Id", conversationId.ToString() },
                        { "Owner Id", GetOwnerId().ToString() }
                    };

                    _hist.AddMessage(AuthorRole.User, col, Encoding.ASCII, p);
                }
            }



            var settings = new PromptExecutionSettings
            {

                FunctionChoiceBehavior = FunctionChoiceBehavior.Required(autoInvoke: true),
                ExtensionData = new Dictionary<string, object>()
                {
                    ["Conversation Id"] = conversationId.ToString(),
                    ["Owner Id"] = GetOwnerId().ToString(),
                },
                ModelId = Config.Instance.Model
            };

            StaticHelpers.SaveChatHistory(conversationId, _hist);


            //Get the streaming chat message contents

            

            IReadOnlyList<ChatMessageContent> result = await _semanticKernelService.GetChatService().GetChatMessageContentsAsync(
                _hist,
                kernel: _semanticKernelService.GetKernel(),
                executionSettings: settings
            );

            _hist = StaticHelpers.GetChatHistory(conversationId);
            StringBuilder output = new StringBuilder();
            foreach (ChatMessageContent content in result) output.AppendLine(content.Content);

            string op = output.ToString();
            _hist.AddAssistantMessage(op);

            StaticHelpers.SaveChatHistory(conversationId, _hist);


            return new ContentResult
            {
                Content = BuildOutput(_hist),
                ContentType = "text/html"
            };
        }
    }
}
