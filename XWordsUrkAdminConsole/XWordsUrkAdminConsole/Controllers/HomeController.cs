using System;
using System.Collections.Generic;
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

            if ((model!=null && model.Id == reqUser.Id) || reqUser.HasRole(UserRoles.UsersAdmin.ToString()))
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
    }
}