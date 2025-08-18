using AICore.Classes;
using AICore.Hubs;
using AICore.SemanticKernel.Extensions;
using AICore.Service;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using OllamaSharp;

namespace AICore.SemanticKernel;

public static class SemanticKernelExtensions
{
    public static void AddSemanticKernel(this WebApplicationBuilder builder)
    {
        KernelMemoryBuilderBuildOptions kmbOptions = new()
        {
            AllowMixingVolatileAndPersistentData = true
        };

        IKernelMemory memory = new KernelMemoryBuilder(new ServiceCollection())
            .WithOllamaTextEmbeddingGeneration(Config.Instance.EmbeddingModel, Config.Instance.OllamaServerUrl)
            .WithOllamaTextGeneration(Config.Instance.Model, Config.Instance.OllamaServerUrl)
            .WithSearchClientConfig(new SearchClientConfig
            {
                EmptyAnswer =
                    "I'm sorry, I haven't found any relevant information that can be used to answer your question",
                MaxMatchesCount = 50,
                AnswerTokens = 2000
            })
            .WithCustomTextPartitioningOptions(new TextPartitioningOptions
            {
                // Defines the properties that are used to split the documents in chunks.
                MaxTokensPerParagraph = 2000,
                OverlappingTokens = 200
            })
            .WithPostgresMemoryDb(new PostgresConfig { ConnectionString = Config.Instance.ConnectionString })
            .Build<MemoryServerless>(kmbOptions);


        HttpClient httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        httpClient.BaseAddress = new Uri(Config.Instance.OllamaServerUrl);
        OllamaApiClient client = new OllamaApiClient(httpClient, Config.Instance.Model);


        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder.Services.AddSingleton<SearXNGSearchPlugin>();
        


        //Register the Ollama client
        kernelBuilder.AddOllamaChatCompletion(client);

        // Register the Ollama text generation and embedding generation clients
        kernelBuilder.AddOllamaTextGeneration(client);

        // Register the Ollama chat client
        kernelBuilder.AddOllamaChatClient(client);


        OllamaApiClient embeddingClient = new OllamaApiClient(httpClient, Config.Instance.EmbeddingModel);
        // Register the Ollama embedding generation client
        kernelBuilder.AddOllamaEmbeddingGenerator(embeddingClient);


        kernelBuilder.Plugins.AddFromType<TimeInformationPlugin>();
        kernelBuilder.Plugins.AddFromType<TimePlugin>();
        
        

        Kernel kernel = kernelBuilder.Build();


        MemoryPlugin plugin = new MemoryPlugin(memory, "kernelMemory", waitForIngestionToComplete: true);

        kernel.ImportPluginFromObject(plugin, "memory");
        kernel.ImportPluginFromType<TestPlugin>();


        builder.Services.AddSingleton(kernel);
        builder.Services.AddSingleton(memory);
        builder.Services.AddSingleton<ISemanticKernelService, SemanticKernelService>();
        //builder.Services.AddTransient<ISemanticKernelService, SemanticKernelService>();

        var scope = builder.Services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
        var ikernel = builder.Services.BuildServiceProvider().GetRequiredService<ISemanticKernelService>();
        var backend = builder.Services.BuildServiceProvider().GetRequiredService<IBackend>();
        
        
        
        


        SearXNGSearchPlugin internetSearchPlugin = new SearXNGSearchPlugin(ikernel,backend);
        InternetUrlLoadPlugin internetUrlLoadPlugin = new InternetUrlLoadPlugin(ikernel, backend);
        ComfyPlugin comfyPlugin = new ComfyPlugin(ikernel, backend);

        kernel.ImportPluginFromObject(internetSearchPlugin, "Internet_Search");
        //kernel.ImportPluginFromType<SearXNGSearchPlugin>();
        kernel.ImportPluginFromObject(internetUrlLoadPlugin, "internetUrlLoadPlugin");
        kernel.ImportPluginFromObject(comfyPlugin, "ComfyPlugin");

        //builder.Services.AddSingleton(internetSearchPlugin);
        builder.Services.AddSingleton(internetUrlLoadPlugin);
        builder.Services.AddSingleton(comfyPlugin);




    }
}