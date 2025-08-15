using AICore.Classes;
using AICore.SemanticKernel.Extensions;
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
        builder.Services.AddTransient<ISemanticKernelService, SemanticKernelService>();


        SearXNGSearchPlugin internetSearchPlugin = new SearXNGSearchPlugin(
            builder.Services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(),
            builder.Services.BuildServiceProvider().GetRequiredService<ISemanticKernelService>());


        InternetUrlLoadPlugin internetUrlLoadPlugin = new InternetUrlLoadPlugin(
            builder.Services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(),
            builder.Services.BuildServiceProvider().GetRequiredService<ISemanticKernelService>());


        ComfyPlugin comfyPlugin = new ComfyPlugin(
            builder.Services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>());


        kernel.ImportPluginFromObject(internetSearchPlugin, "Internet_Search");
        kernel.ImportPluginFromObject(internetUrlLoadPlugin, "internetUrlLoadPlugin");
        kernel.ImportPluginFromObject(comfyPlugin, "ComfyPlugin");
    }
}