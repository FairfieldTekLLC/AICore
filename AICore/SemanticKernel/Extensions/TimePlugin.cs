using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace AICore.SemanticKernel.Extensions;

public class TimeInformationPlugin
{
    [KernelFunction("get-time")]
    [Description("returns the current time.")]
    public async Task<string> GetTime()
    {
        Console.WriteLine("Fetching Time");
        return DateTime.Now.ToString("HH:mm:ss zz");
    }


    [KernelFunction("get-date")]
    [Description("returns the current date.")]
    public async Task<string> GetDate()
    {
        Console.WriteLine("Fetching Date");
        return DateTime.Now.ToString("HH:mm:ss zz");
    }
}