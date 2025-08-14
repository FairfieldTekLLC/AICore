using AICore.Classes;
using AICore.Models;
using AICore.SemanticKernel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Diagnostics;

namespace AICore.Controllers
{
    public class BaseController : Controller
    {
        public BaseController(ILogger<HomeController> logger, ISemanticKernelService semanticKernelService)
        {
            _logger = logger;
            _semanticKernelService = semanticKernelService;
        }
        public readonly ILogger<HomeController> _logger;
        public readonly ISemanticKernelService _semanticKernelService;
        public Guid GetOwnerId()
        {
            string key = HttpContext.Session.GetString("UserKey");
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
            if (context.ActionDescriptor.DisplayName.Equals("AICore.Controllers.LoginController.LoginOrCreate (AICore)"))
                return;
            Debug.WriteLine(context.Controller.ToString());
            string key = HttpContext.Session.GetString("UserKey");
            try
            {
                var t = new Guid(key);
            }
            catch (Exception e)
            {
                Response.Redirect("/Login/LoginOrCreate");
            }
        }
    }
}
