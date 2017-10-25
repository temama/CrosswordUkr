using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XWordsUrkAdminConsole.Accounting;

namespace XWordsUrkAdminConsole.Controllers
{
    public class GamesController : Controller
    {
        // GET: Games
        public ActionResult Index()
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return RedirectToAction("Login", "Home", new { ReturnUrl = Request.RawUrl });

            return View();
        }

        public ActionResult Games()
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return RedirectToAction("Login", "Home", new { ReturnUrl = Request.RawUrl });

            return View("Games");
        }
    }
}