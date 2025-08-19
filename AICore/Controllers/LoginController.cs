using AICore.Classes;
using AICore.Models;
using AICore.SemanticKernel;
using AICore.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Console;

namespace AICore.Controllers;

public class LoginController(ILogger<HomeController> logger, ISemanticKernelService semanticKernelService)
    : BaseController(logger,
        semanticKernelService)
{
    public IActionResult Login()
    {
        return View();
    }

    public IActionResult CreateAccount()
    {
        return PartialView("CreateAccount");
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return Json(new LoginResultVm { Authorized = false, Message = "Invalid password" });


        using NewsReaderContext ctx = new NewsReaderContext();
        if (ctx.Securityobjects.Any(x => x.Username == username))
        {
            if (ctx.Securityobjects.Any(x => x.Username== username && x.Pass == password))
            {
                Securityobject? so =
                    ctx.Securityobjects.FirstOrDefault(x => x.Username == username && x.Pass == password);
                HttpContext.Session.SetString("UserKey", so.Activedirectoryid.ToString());
                return Json(new LoginResultVm { Authorized = true, Message = "Authorized" });
            }

            return Json(new LoginResultVm { Authorized = false, Message = "Invalid password" });
        }

        
        return Json(new LoginResultVm { Authorized = false, Message = "Username or password is incorrect." });
    }

    [HttpPost]
    public IActionResult CreateAccount(string username, string password, string emailAddress, string forename, string surname)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(emailAddress) || string.IsNullOrEmpty(forename) || string.IsNullOrEmpty(surname))
            return Json(new LoginResultVm { Authorized = false, Message = "All information is required." });

        if (username.Trim().Length<5)
            return Json(new LoginResultVm { Authorized = false, Message = "Username is too short, minimum of 5 characters" });









        using NewsReaderContext ctx = new NewsReaderContext();
        var chk = ctx.Securityobjects.Any(x=>x.Username==username);
        if (chk)
            return Json(new LoginResultVm { Authorized = false, Message = "Username already in use." });

        if (!emailAddress.IsValidEmailAddress())
            return Json(new LoginResultVm { Authorized = false, Message = "Invalid Email Address." });

        Securityobject so = new Securityobject()
        {
            Username = username,
            Activedirectoryid = Guid.NewGuid(),
            Emailaddress = emailAddress,
            Forename = forename,
            Surname = surname,
            Fullname = surname + ", " + forename,
            Isactive = 1,
            Isgroup = 0,
            Pass = password
        };
        ctx.Securityobjects.Add(so);
        ctx.SaveChanges();
        return Json(new LoginResultVm { Message = "Account Created", Authorized = true });
    }
}