using AICore.SemanticKernel;

namespace AICore.Service
{
    public interface IBackend
    {
        ISemanticKernelService getISemanticKernelService();
        void SendMessage(Guid conversationId, string message);
    }
}
