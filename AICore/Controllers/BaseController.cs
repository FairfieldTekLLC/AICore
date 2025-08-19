using System.Diagnostics;
using AICore.SemanticKernel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AICore.Controllers;

public class BaseController(ILogger<HomeController> logger, ISemanticKernelService semanticKernelService)
    : Controller
{
    public readonly ILogger<HomeController> _logger = logger;
    public readonly ISemanticKernelService _semanticKernelService = semanticKernelService;

    public Guid GetOwnerId()
    {
        string? key = HttpContext.Session.GetString("UserKey");
        if (key == null)
            return Guid.Empty;
        try
        {
            return new Guid(key);
        }
        catch (Exception e)
        {
            return Guid.Empty;
        }
    }


    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.ActionDescriptor.DisplayName.Equals("AICore.Controllers.LoginController.Login (AICore)")
            || (context.ActionDescriptor.DisplayName.Equals("AICore.Controllers.LoginController.CreateAccount (AICore)"))
            )
            return;

        Debug.WriteLine(context.Controller.ToString());
        string? key = HttpContext.Session.GetString("UserKey");
        try
        {
            Guid t = new Guid(key);
        }
        catch (Exception e)
        {
            Response.Redirect("/Login/Login");
        }
    }
}