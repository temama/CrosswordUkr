using XWordsUrkAdminConsole.Models;
using System.Web.Mvc;
using System.Linq;

namespace XWordsUrkAdminConsole.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Words()
        {
            using (var context = new XWordsAdminModelContext())
            {
                return View(context.Words.Select(w=>w.TheWord.StartsWith("Г")).ToList());
            }
        }
    }
}