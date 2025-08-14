using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Diagnostics;

namespace AICore.SemanticKernel.Extensions
{
    public class TestPlugin
    {
        [KernelFunction("Test")]
        [Description("do a test")]
        public async Task<string> Work(
            [Description("The conversation Id")] Guid conversationId,
            [Description("Owner Id")] Guid ownerId
            //,[Description("of")] string imageDescription
            )
        {
            //Debug.WriteLine(conversationId);
            //Debug.WriteLine(ownerId);
            //Debug.WriteLine(imageDescription);
            return "ok";
        }
    }
}
