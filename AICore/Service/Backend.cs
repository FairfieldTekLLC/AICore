using AICore.Classes;
using AICore.Hubs;
using AICore.SemanticKernel;
using Microsoft.AspNetCore.SignalR;
using OllamaSharp.Models.Chat;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace AICore.Service
{
    public class Backend : BackgroundService, IBackend
    {
        public class test()
        {
            public Guid ConversationId { get; set; }
            public string Message { get; set; }
        }

        public static System.Collections.Concurrent.ConcurrentQueue<test> msgs = new ConcurrentQueue<test>();


        private static  ISemanticKernelService _semanticKernelService;
        private static  IHubContext<ChatHub> _hubContent;
        public Backend(ISemanticKernelService semanticKernelService, IHubContext<ChatHub> hubContext)
        {
            _semanticKernelService = semanticKernelService;
            _hubContent = hubContext;
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    if (msgs.TryDequeue(out var tosend))
                    {


                        _hubContent.Clients.Groups(tosend.ConversationId.ToString()).SendCoreAsync(
                            "ReceiveMessage", new[]
                                { tosend.ConversationId.ToString(), tosend.Message }).GetAwaiter().GetResult();
                    }

                    Thread.Sleep(100);
                }
            });
            t.Start();
        }
        public ISemanticKernelService getISemanticKernelService()
        {
            return _semanticKernelService;
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Debug.WriteLine("help");
            return Task.CompletedTask;
        }

        public void SendMessage(Guid conversationId, string message)
        {
            msgs.Enqueue(new test() { ConversationId = conversationId, Message = message });
        }
    }
}
