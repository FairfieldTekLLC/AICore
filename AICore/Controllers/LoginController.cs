using AICore.Models;
using AICore.SemanticKernel;
using AICore.ViewModels;
using DocumentFormat.OpenXml.Math;
using Microsoft.AspNetCore.Mvc;

namespace AICore.Controllers
{
    public class LoginController : BaseController
    {
        public LoginController(ILogger<HomeController> logger, ISemanticKernelService semanticKernelService) : base(logger, semanticKernelService)
        {
        }

        public IActionResult LoginOrCreate()
        {
            return View();
        }

        public IActionResult LoginOrCreateFunction(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return Json(new LoginResultVm { Authorized = false, Message = "Invalid password" });
            using (var ctx = new NewsReaderContext())
            {
                if (ctx.Securityobjects.Any(x => x.Emailaddress == email))
                {
                    if (ctx.Securityobjects.Any(x => x.Emailaddress == email && x.Pass == password))
                    {
                        var so = ctx.Securityobjects.FirstOrDefault(x => x.Emailaddress == email && x.Pass == password);
                        HttpContext.Session.SetString("UserKey", so.Activedirectoryid.ToString());
                        return Json(new LoginResultVm { Authorized = true, Message = "Authorized" });
                    }

                    return Json(new LoginResultVm { Authorized = false, Message = "Invalid password" });
                }

                Securityobject o = new Securityobject
                {
                    Emailaddress = email,
                    Pass = password,
                    Activedirectoryid = Guid.NewGuid(),
                    Forename = "",
                    Fullname = "",
                    Isactive = 1,
                    Isgroup = 0,
                    Surname = "",
                    Username = ""
                };
                ctx.Securityobjects.Add(o);
                ctx.SaveChanges();
                return Json(new LoginResultVm { Authorized = true, Message = "Account Created" });
            }
        }
    }
}
