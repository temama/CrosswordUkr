using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using XWordsUrkAdminConsole.Accounting;
using XWordsUrkAdminConsole.Helpers;
using XWordsUrkAdminConsole.Models;

namespace XWordsUrkAdminConsole.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return RedirectToAction("Login", "Home", new { ReturnUrl = Request.RawUrl });

            using (var dbContext = new XWordsAdminModelContext())
            {
                var model = new DashboardData
                {
                    TotalWords = dbContext.Words.Count(),
                    TotalClues = dbContext.Clues.Count(),
                    TotalGames = dbContext.Games.Count(),
                    WordsProcessed = dbContext.Words.Count(w => w.State != WordState.None)
                };

                model.WordsStates = new Dictionary<WordState, int>();
                foreach (WordState e in Enum.GetValues(typeof(WordState)))
                {
                    model.WordsStates[e] = dbContext.Words.Count(w => w.State == e);
                }

                model.CluesStates = new Dictionary<ClueState, int>();
                foreach (ClueState e in Enum.GetValues(typeof(ClueState)))
                {
                    model.CluesStates[e] = dbContext.Clues.Count(c => c.State == e);
                }

                model.GamesStates = new Dictionary<GameState, int>();
                foreach (GameState e in Enum.GetValues(typeof(GameState)))
                {
                    model.GamesStates[e] = dbContext.Games.Count(c => c.State == e);
                }

                return View(model);
            }
        }

        public ActionResult About()
        {
            return PartialView();
        }

        public ActionResult Contact()
        {
            return PartialView();
        }

        public ActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel() { ReturnUrl = returnUrl, RememberMe = true });
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    AuthModule.Login(model.LoginName, model.Password, model.RememberMe, Response);
                    if (string.IsNullOrEmpty(model.ReturnUrl))
                        return RedirectToAction("Index", "Home");

                    return Redirect(model.ReturnUrl);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Login error: " + ex.Message);
                }
            }

            return View(model);
        }

        [HttpPost]
        public JsonResult UpdatePassword(string oldPassword, string newPassword)
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return Json("Error: Please login to proceed"); //RedirectToAction("Login", "Home", new { ReturnUrl = Request.RawUrl });

            try
            {
                AuthModule.UpdatePassword(reqUser.Id, oldPassword, newPassword);
                return Json("Password updated", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("Error: " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [ChildActionOnly]
        public ActionResult UserMenu()
        {
            return PartialView(AuthModule.GetCurrentUser(Request, true, Response));
        }

        public ActionResult MyUserDetails()
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return RedirectToAction("Login", "Home");

            reqUser.PasswordHash = string.Empty;
            return PartialView("UserDetails", reqUser);
        }

        public ActionResult UserDetails(User model)
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return RedirectToAction("Login", "Home");

            if ((model != null && model.Id == reqUser.Id) || reqUser.HasRole(UserRoles.UsersAdmin.ToString()))
            {
                try
                {
                    using (var dbContext = new XWordsAdminModelContext())
                    {
                        var dbUser = model.Id > 0 ? dbContext.Users.FirstOrDefault(u => u.Id == model.Id) :
                            dbContext.Users.Add(new User());

                        dbUser.Login = model.Login;
                        dbUser.Initials = model.Initials;
                        if (dbUser.Email != model.Email)
                            dbUser.EmailConfirmed = false;
                        dbUser.Email = model.Email;
                        dbUser.Roles = model.Roles;
                        dbUser.Valid = model.Valid;
                        dbUser.Settings = model.Settings;

                        dbContext.SaveChanges();
                        return PartialView(dbUser);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return PartialView(model);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "You are not authorised to edit this user");
                return PartialView(model);
            }
        }

        [HttpPost]
        public JsonResult SaveUserDetails(User model)
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return Json("Error: Please login to proceed");

            if ((model != null && model.Id == reqUser.Id) || reqUser.HasRole(UserRoles.UsersAdmin.ToString()))
            {
                try
                {
                    using (var dbContext = new XWordsAdminModelContext())
                    {
                        var dbUser = model.Id > 0 ? dbContext.Users.FirstOrDefault(u => u.Id == model.Id) :
                            dbContext.Users.Add(new User());

                        dbUser.Login = model.Login;
                        dbUser.Initials = model.Initials;
                        if (dbUser.Email != model.Email)
                            dbUser.EmailConfirmed = false;
                        dbUser.Email = model.Email;
                        // TODO: Add later. Not needed now
                        //dbUser.Roles = model.Roles;
                        //dbUser.Valid = model.Valid;
                        //dbUser.Settings = model.Settings;

                        dbContext.SaveChanges();
                        return Json("User details saved");
                    }
                }
                catch (Exception ex)
                {
                    return Json("Error: " + ex.Message);
                }
            }
            else
            {
                return Json("Error: You are not authorised to edit this user");
            }
        }

        public ActionResult Logout()
        {
            AuthModule.LogOut(Response);
            return RedirectToAction("Login", "Home");
        }

        //[HttpPost]
        //public JsonResult GetWordsStates()
        //{
        //    var res = new Chart();
        //}

        [HttpPost]
        public ActionResult GetEventsFeed(int count, int? skipFirstN)
        {
            return PartialView("EventsFeed", DashboardHelper.GetEventsFeed(Url, count, skipFirstN));
        }

        public JsonResult GetWeeklyActivity()
        {
            var aLabels = new List<string>();
            var aData = new List<int>();
            using (var dbContext = new XWordsAdminModelContext())
            {
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Now.AddDays(-i);
                    aLabels.Add(DashboardHelper.GetVeryShortDayRepr(date.DayOfWeek));
                    aData.Add(dbContext.Events.Where(e => DbFunctions.TruncateTime(e.TimeStamp) == date.Date).Count());
                }
            }
            return Json(new { labels = aLabels, data = aData }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEvents(DateTime start, DateTime end)
        {
            //var startDate = CoreHelper.UnixTimeStampToDateTime(start);
            //var endDate = CoreHelper.UnixTimeStampToDateTime(end);
            var startDate = start;
            var endDate = end;

            using (var dbContext = new XWordsAdminModelContext())
            {
                var evs = dbContext.Events.Where(e => e.Table == "Events"
                    && e.TimeStamp >= startDate
                    && e.TimeStamp < endDate).ToList();

                var evsFormated = evs.Select(e => new
                {
                    id = e.Id,
                    title = e.Comment,
                    start = e.TimeStamp.Date.ToString("yyyy-MM-dd"),
                    type = e.RecordId
                    //end = CoreHelper.ToUnixTime(e.TimeStamp + TimeSpan.FromMinutes(15)),//(e.TimeStamp + TimeSpan.FromMinutes(15)).ToString("s"),
                    //className = "btn btn-primary", // add different classes depends on RecordId
                    ////someKey = 0,
                    //allDay = true
                });

                return Json(evsFormated.ToArray(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EventDetails(int? id)
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return RedirectToAction("Login", "Home");

            if (id == null || !id.HasValue || id.Value <= 0)
                return PartialView("EventDetails", new Event() {
                    Table = "Events",
                    User = reqUser,
                    UserId = reqUser.Id,
                    TimeStamp = DateTime.Now
                });

            using (var dbContext = new XWordsAdminModelContext())
            {
                var ev = dbContext.Events.Include(e => e.User).FirstOrDefault(e => e.Id == id.Value);
                return PartialView("EventDetails", ev);
            }
        }
    }
}