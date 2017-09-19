using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XWordsUrkAdminConsole.Helpers;
using XWordsUrkAdminConsole.Models;

namespace XWordsUrkAdminConsole.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (!LoginHelper.IsLoggedIn())
                return View("Login", new LoginViewModel() { ReturnUrl = Request.RawUrl });
            
            return View();
        }

        public ActionResult About()
        {
            return PartialView();
        }

        public ActionResult Contact()
        {
            return PartialView();
        }

        public ActionResult Login()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult UserMenu()
        {
            return PartialView(LoginHelper.IsLoggedIn());
        }
    }
}