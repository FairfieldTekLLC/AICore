using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using AICore.Classes;
using AICore.Controllers.ViewModels;
using AICore.Models;
using AICore.SemanticKernel;
using AICore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using OllamaSharp;
using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;
using TextContent = Microsoft.SemanticKernel.TextContent;

namespace AICore.Controllers;

public class HomeController : BaseController
{
   
    
    public HomeController(ILogger<HomeController> logger, ISemanticKernelService semanticKernelService) : base(logger, semanticKernelService)
    {
       
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    


}