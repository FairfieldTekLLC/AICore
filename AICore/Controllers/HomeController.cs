using System.Diagnostics;
using AICore.SemanticKernel;
using AICore.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AICore.Controllers;

public class HomeController(ILogger<HomeController> logger, ISemanticKernelService semanticKernelService)
    : BaseController(logger,
        semanticKernelService)
{
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