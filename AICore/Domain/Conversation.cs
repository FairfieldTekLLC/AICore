//using System.Collections.Concurrent;
//using System.Diagnostics;
//using System.Text;
//using AICore.Domain.Data_Classes;
//using AICore.Models;
//using AICore.SemanticKernel;
//using Microsoft.AspNetCore.StaticFiles;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.ChatCompletion;
//using Microsoft.SemanticKernel.Connectors.Ollama;

//namespace AICore.Domain;

//public class Conversation
//{
//    public static int MaxNumberOfResults = 5;


//    public Guid Id { get; set; } = Guid.Empty;
//    public Guid? ParentId { get; set; } = Guid.Empty;
//    public string? Title { get; set; }
//    public DateTime CreatedAt { get; set; }
//    public string? Description { get; set; }
//    public Guid OwnerId { get; set; } = Guid.Empty;

//    public List<Entry> PromptsOrSearches { get; set; } = new();

//    public void ClearCachedSearchResults()
//    {
//        foreach (Entry entry in PromptsOrSearches)
//        {
//            if (entry.FetchedDocuments.Count > 0)
//                entry.FetchedDocuments.Clear();
//            ParentId = entry.Id;
//            entry.Id = Guid.Empty;
//        }
//    }


//    public void Load(Guid conversationId)
//    {
//        using (NewsReaderContext ctx = new NewsReaderContext())
//        {
//            Models.Conversation? dat = ctx.Conversations
//                .Include(x => x.Entries)
//                .ThenInclude(x => x.Fetcheddocs)
//                .FirstOrDefault(x => x.Pkconversationid == conversationId);

//            if (dat == null)
//                throw new Exception("Can't find conversation!");

//            Id = dat.Pkconversationid;
//            Description = dat.Description;
//            CreatedAt = dat.Createdat;
//            Title = dat.Title;
//            OwnerId = dat.Fksecurityobjectowner;
//            ParentId = dat.Fkparentid;

//            PromptsOrSearches.Clear();

//            foreach (Models.Entry? entry in dat.Entries)
//            {
//                Entry e = new Entry
//                {
//                    Id = entry.Pkentryid,
//                    NumberOfResults = entry.Numberofresults ?? 0,
//                    Text = entry.Text,
//                    ResultText = entry.Resulttext,
//                    Role = (Role)entry.Fkroleid,
//                    Type = (ConversationType)entry.Fkconversationtypeid,
//                    FetchedDocuments = [],
//                    Sequence = entry.Sequence,
//                    FileData = entry.Filedata,
//                    IsHidden = entry.Ishidden == 1
//                };


//                e.FetchedDocuments = (from doc in entry.Fetcheddocs
//                                      select new HTMLDocs
//                                      {
//                                          Body = doc.Body,
//                                          Id = doc.Pkfetchdocid,
//                                          MemoryKey = doc.Memorykey,
//                                          Uri = doc.Uri,
//                                          Summary = doc.Summary
//                                      }).ToList();

//                PromptsOrSearches.Add(e);
//            }
//        }
//    }


//    public void Save()
//    {
//        if (Id == Guid.Empty)
//            //Ok New Conversation, create it
//            using (NewsReaderContext ctx = new NewsReaderContext())
//            {
//                Models.Conversation dat = new Models.Conversation
//                {
//                    Createdat = DateTime.Now,
//                    Description = Description,
//                    Title = Title,
//                    Fksecurityobjectowner = OwnerId
//                };
//                ctx.Conversations.Add(dat);
//                ctx.SaveChanges();
//                Id = dat.Pkconversationid;
//            }
//        else
//            using (NewsReaderContext ctx = new NewsReaderContext())
//            {
//                //Update existing conversation
//                Models.Conversation? dat = ctx.Conversations.FirstOrDefault(x => x.Pkconversationid == Id);
//                if (dat != null)
//                {
//                    dat.Title = Title;
//                    dat.Description = Description;
//                    dat.Createdat = DateTime.Now;
//                    dat.Fksecurityobjectowner = OwnerId;
//                    ctx.SaveChanges();
//                }
//            }

//        using (NewsReaderContext ctx = new NewsReaderContext())
//        {
//            //Save entries
//            foreach (Entry entry in PromptsOrSearches.ToList())
//            {
//                if (entry.Id == Guid.Empty)
//                {
//                    Models.Entry ent = new Models.Entry
//                    {
//                        Pkentryid = Guid.NewGuid(),
//                        Fkconversationid = Id,
//                        Fkconversationtypeid = (int)entry.Type,
//                        Fkroleid = (int)entry.Role,
//                        Text = entry.Text,
//                        Numberofresults = entry.NumberOfResults,
//                        Resulttext = entry.ResultText,
//                        Sequence = entry.Sequence,
//                        Filedata = entry.FileData,
//                        Ishidden = entry.IsHidden ? (short)1 : (short)0
//                    };
//                    ctx.Entries.Add(ent);
//                    ctx.SaveChanges();
//                    entry.Id = ent.Pkentryid; //Set the Id back to the entry
//                }
//                else
//                {
//                    //Update existing entry
//                    Models.Entry? ent = ctx.Entries.FirstOrDefault(x => x.Pkentryid == entry.Id);
//                    if (ent != null)
//                    {
//                        ent.Fkconversationtypeid = (int)entry.Type;
//                        ent.Fkroleid = (int)entry.Role;
//                        ent.Text = entry.Text;
//                        ent.Numberofresults = entry.NumberOfResults;
//                        ent.Resulttext = entry.ResultText;
//                        ent.Sequence = entry.Sequence;
//                        ent.Filedata = entry.FileData;
//                        ent.Ishidden = entry.IsHidden ? (short)1 : (short)0;
//                        ctx.SaveChanges();
//                    }
//                }

//                SaveFetchedDocs(entry);
//            }

//            ctx.SaveChanges();
//        }
//    }

//    private void SaveFetchedDocs(Entry entry)
//    {
//        using (NewsReaderContext ctx = new NewsReaderContext())
//        {
//            List<Fetcheddoc> toDelete = ctx.Fetcheddocs.Where(x => x.Fkentryid == entry.Id).ToList();
//            foreach (Fetcheddoc doc in toDelete) ctx.Fetcheddocs.Remove(doc);


//            foreach (HTMLDocs doc in entry.FetchedDocuments)
//            {
//                Fetcheddoc fetchedDoc = new Fetcheddoc
//                {
//                    Pkfetchdocid = Guid.NewGuid(),
//                    Fkentryid = entry.Id,
//                    Uri = doc.Uri,
//                    Body = doc.Body,
//                    Memorykey = doc.MemoryKey,
//                    Summary = doc.Summary
//                };
//                ctx.Fetcheddocs.Add(fetchedDoc);
//                ctx.SaveChanges();
//                doc.Id = fetchedDoc.Pkfetchdocid;
//            }
//        }
//    }


//    public void WriteLog(string msg)
//    {
//        Debug.WriteLine(msg);
//        Console.WriteLine(msg);
//    }

//    public async Task ImportText(Entry item, ISemanticKernelService kernal, IBackendWorker worker)
//    {
//        if (item.FetchedDocuments.Count == 0)
//        {
//            await worker.SendMessage(Id, "Importing Text.....");
//            try
//            {
//                HTMLDocs doc = new HTMLDocs
//                {
//                    Uri = "",
//                    Body = "",
//                    MemoryKey = await kernal.ImportText(item.Text, Id, OwnerId),
//                    Summary = ""
//                };

//                item.FetchedDocuments = [doc];
//                await worker.SendMessage(Id, "Finished importing text!");
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//            }
//        }
//    }

//    public async Task ImportWebSearch(Entry item, ISemanticKernelService kernal, IBackendWorker worker)
//    {
//        if (item.FetchedDocuments.Count == 0)
//        {
//            await worker.SendMessage(Id, "Performing Web Search for: " + item.Text);
//            WriteLog($"Performing web search: {item.Text} with role: {item.Role}");
//            string searchString = item.Text ?? "";
//            if (searchString.Contains(" today ", StringComparison.InvariantCultureIgnoreCase))
//                searchString = searchString.Replace(" today ", DateTime.Now.ToString("D"));
//            if (searchString.Contains(" today?", StringComparison.InvariantCultureIgnoreCase))
//                searchString = searchString.Replace(" today?", DateTime.Now.ToString("D"));
//            if (searchString.Contains(" today.", StringComparison.InvariantCultureIgnoreCase))
//                searchString = searchString.Replace(" today.", DateTime.Now.ToString("D"));
//            if (searchString.Contains(" today!", StringComparison.InvariantCultureIgnoreCase))
//                searchString = searchString.Replace(" today!", DateTime.Now.ToString("D"));
//            if (searchString.Contains(" today", StringComparison.InvariantCultureIgnoreCase))
//                searchString = searchString.Replace(" today", DateTime.Now.ToString("D"));

//            if (searchString.Contains(" today's", StringComparison.InvariantCultureIgnoreCase))
//                searchString = searchString.Replace(" today's", DateTime.Now.ToString("D"));

//            var Foundurls = new List<string> { searchString }.QuerySearchEngineForUrls(30);

//            List<string> urls = new List<string>();

//            int counter = 0;
//            foreach (var url in Foundurls)
//            {
//                if (!url.url.Contains("nytimes", StringComparison.InvariantCultureIgnoreCase))
//                {
//                    urls.Add(url.url);
//                    counter++;
//                }

//                if (counter >= MaxNumberOfResults)
//                    break;
//            }


//            ConcurrentBag<HTMLDocs> hEntries = new ConcurrentBag<HTMLDocs>();

//            Parallel.ForEach(urls, new ParallelOptions { MaxDegreeOfParallelism = 8 }, url =>
//            {
//                try
//                {
//                    worker.SendMessage(Id, "Started Processing url: " + url).ConfigureAwait(false).GetAwaiter()
//                        .GetResult();

//                    Debug.WriteLine("Processing url: " + url);
//                    HTMLDocs doc = new HTMLDocs
//                    {
//                        Uri = url,
//                        Body = "",
//                        Summary = "",
//                        MemoryKey = kernal.ImportWebPage(url, Id, OwnerId).Result
//                    };

//                    Debug.WriteLine("Finished loading file! " + url);
//                    worker.SendMessage(Id, "Finished Processing url: " + url).ConfigureAwait(false).GetAwaiter()
//                        .GetResult();

//                    hEntries.Add(doc);
//                }
//                catch (Exception e)
//                {
//                    try
//                    {
//                        string content = url.FetchUrlAsContent().Result;
//                        if (string.IsNullOrEmpty(content) || content.Length < 500)
//                        {
//                            Console.WriteLine("Failed to fetch content for URL: " + url);
//                            return;
//                        }

//                        HTMLDocs doc = new HTMLDocs
//                        {
//                            Uri = url,
//                            Body = "",
//                            MemoryKey = content,
//                            Summary = ""
//                        };
//                        hEntries.Add(doc);
//                    }
//                    catch (Exception exception)
//                    {
//                        Console.WriteLine(exception);
//                    }
//                }
//            });

//            item.FetchedDocuments = hEntries.Where(x => x.MemoryKey != string.Empty).ToList();
//        }
//    }

//    public async Task ImportUrl(Entry item, ISemanticKernelService kernal, IBackendWorker worker)
//    {
//        if (item.FetchedDocuments.Count == 0)
//        {
//            await worker.SendMessage(Id, "Performing web page import: " + item.Text);

//            try
//            {
//                HTMLDocs doc = new HTMLDocs
//                {
//                    Uri = item.Text,
//                    Body = "",
//                    MemoryKey = await kernal.ImportWebPage(item.Text, Id, OwnerId),
//                    Summary = ""
//                };

//                item.FetchedDocuments = [doc];
//                await worker.SendMessage(Id, "Finished web page import: " + item.Text);
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//            }
//        }
//    }

//    public async Task ImportFile(Entry item, ISemanticKernelService kernal, IBackendWorker worker)
//    {
//        if (item.FetchedDocuments.Count == 0)
//        {
//            await worker.SendMessage(Id, "Performing PDF import: " + item.Text);

//            try
//            {
//                using (Stream stream = new MemoryStream(item.FileData))
//                {
//                    HTMLDocs doc = new HTMLDocs
//                    {
//                        Uri = item.Text,
//                        Body = "",
//                        MemoryKey = await kernal.ImportFile(item.Text, stream, Id, OwnerId),
//                        Summary = ""
//                    };
//                    item.FetchedDocuments = [doc];
//                    await worker.SendMessage(Id, "Finished Performing PDF import: " + item.Text);
//                }
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//            }
//        }
//    }


//    public string GetMimeTypeForFileExtension(string filePath)
//    {
//        const string DefaultContentType = "application/octet-stream";

//        var provider = new FileExtensionContentTypeProvider();

//        if (!provider.TryGetContentType(filePath, out string contentType))
//        {
//            contentType = DefaultContentType;
//        }

//        return contentType;
//    }


//public async Task RunConversation(ISemanticKernelService kernal, IBackendWorker worker)
//    {
//        await worker.SendMessage(Id, "Starting conversation.....");
//        List<Message> messages = new List<Message>();


//        foreach (Entry item in PromptsOrSearches.OrderBy(x => x.Sequence))
//        {
//            switch (item.Type)
//            {
//                case ConversationType.ImportText:
//                    {
//                        //No need to send this to the kernel, it is already imported
//                        await ImportText(item, kernal, worker);
//                        break;
//                    }
//                case ConversationType.Prompt:
//                    messages.Add(new Message
//                    {
//                        content = await kernal.Prompt(item.Text),
//                        role = item.Role.ToString().ToLowerInvariant()
//                    });


//                    break;
//                case ConversationType.Ask:
//                case ConversationType.ImageToText:

//                    if (item.ResultText == string.Empty)
//                    {
//                        //Create a empty chat history
//                        await worker.SendMessage(Id, "Creating ChatHistory for conversation: " + Id);

//                        ChatHistory hist = new ChatHistory();

//                        await worker.SendMessage(Id, "Adding System Prompt");

//                        hist.AddSystemMessage(Config.Instance.SystemPrompt);

//                        //Search Long Term Memory

//                        await worker.SendMessage(Id, "Searching long term memory.");


//                        foreach (Message itm in messages)
//                        {
//                            if (itm.role.Equals("ask", StringComparison.InvariantCultureIgnoreCase))
//                                switch (itm.role)
//                                {
//                                    case nameof(Role.assistant):
//                                        hist.AddAssistantMessage(itm.content);

//                                        break;
//                                    case nameof(Role.user):
//                                        hist.AddUserMessage(itm.content);

//                                        break;
//                                    case nameof(Role.system):
//                                        hist.AddSystemMessage(itm.content);
//                                        break;
//                                }
//                        }


//                        string longTermMemory = await kernal.AskAsync(item.Text, Id, OwnerId);
//                        if (longTermMemory != string.Empty)
//                            hist.AddUserMessage("Kernel Memory Answer: " + longTermMemory);

//                        hist.AddSystemMessage("conversation Id: " + Id);
//                        hist.AddSystemMessage("Owner Id: " + OwnerId);


//                        if (item.Type == ConversationType.ImageToText)
//                        {

//                            //ReadOnlyMemory<byte> img = new ReadOnlyMemory<byte>(item.FileData);
//                            //hist.AddMessage(AuthorRole.User, new ChatMessageContentItemCollection()
//                            //{
//                            //    new ImageContent(img,GetMimeTypeForFileExtension()),
//                            //});
//                        }
//                        else
//                        {
//                            hist.AddUserMessage(item.Text);
//                        }


//                            //Get the chat service from the kernel

//                            var chatService = kernal.GetKernel().GetRequiredService<IChatCompletionService>();

//                        await worker.SendMessage(Id, "Processing Chat history.");


//                        var settings = new OllamaPromptExecutionSettings
//                        {
//                            FunctionChoiceBehavior = FunctionChoiceBehavior.Required(autoInvoke: true),
//                            ExtensionData = new Dictionary<string, object>()
//                        };
//                        settings.ExtensionData.Add(new KeyValuePair<string, object>("conversation Id", Id.ToString()));
//                        settings.ExtensionData.Add(new KeyValuePair<string, object>("Owner Id", OwnerId.ToString()));


//                        //Get the streaming chat message contents
//                        var response = await chatService.GetChatMessageContentsAsync(
//                            hist,
//                            kernel: kernal.GetKernel(),
//                            executionSettings: settings
//                        );

//                        StringBuilder output = new StringBuilder();
//                        foreach (ChatMessageContent content in response) output.Append(content.Content);

//                        //SAve the result text
//                        item.ResultText = output.ToString();
//                        worker.SendMessage(Id, "Finished processing conversation");
//                    }
//                    else
//                    {
//                        messages.Add(new Message()
//                        {
//                            content = item.ResultText,
//                            role = nameof(Role.assistant)
//                        });
//                    }

//                    break;
//                case ConversationType.WebSearch:
//                    {
//                        //No need to send this to the kernel, it is already imported
//                        await ImportWebSearch(item, kernal, worker);

//                        messages.Add(new Message()
//                        {
//                            content = item.ResultText ?? "",
//                            role = item.Role.ToString().ToLowerInvariant()
//                        });

//                        break;
//                    }
//                case ConversationType.UrlFetch:
//                    {
//                        //No need to send this to the kernel, it is already imported
//                        await ImportUrl(item, kernal, worker);

//                        break;
//                    }
//                case ConversationType.UploadFile:
//                    {
//                        //No need to send this to the kernel, it is already imported
//                        await ImportFile(item, kernal, worker);

//                        break;
//                    }
//                //case ConversationType.ImageToText:
//                //    {
//                //        if (string.IsNullOrEmpty(item.ResultText))
//                //        {
//                //            SemanticKernelService.OllamaSend msg = new SemanticKernelService.OllamaSend
//                //            {
//                //                model = Config.Instance.VisionModel,
//                //                prompt = item.Text,
//                //                images = [Convert.ToBase64String(item.FileData)],
//                //                stream = false
//                //            };
//                //            var result = await kernal.ProcessOllamaMsg(msg, SemanticKernelService.EndpointType.generate);

//                //            item.ResultText = result.response;
//                //        }

//                //        messages.Add(new Message
//                //        {
//                //            content = item.ResultText,
//                //            role = item.Role.ToString().ToLowerInvariant()
//                //        });

//                //        break;
//                //    }
//            }
//        }
//    }
//}

