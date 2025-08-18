using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AICore.SemanticKernel;

public interface ISemanticKernelService
{
    //public Task SendMessage(Guid conversationId, string message);
    public Task<string> ImportText(string text, Guid conversationId, Guid activeDirectoryId);
    public Task<string> AskAsync(string query, Guid conversationId, Guid activeDirectoryId);
    public Task RemoveFile(string memoryKey, Guid activeDirectoryId);

    public Task<string> ImportWebPage(string url, Guid conversationId, Guid activeDirectoryId);
    public Task<string> ImportFile(string filename, Stream fileData, Guid conversationId, Guid activeDirectoryId);
    public Task<List<Citation>> SearchSummariesAsync(Guid conversationId, Guid activeDirectoryId, string memoryKey);
    public IChatCompletionService GetChatService();
    public Kernel GetKernel();

    public Task<string> Prompt(string prompt);
    //public Task<OllamaResult?> ProcessOllamaMsg(OllamaSend toSend, EndpointType endpointType);
}