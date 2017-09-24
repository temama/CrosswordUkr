using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using XWordsUrkAdminConsole.Models;

namespace XWordsUrkAdminConsole.Accounting
{
    public static class AuthModule
    {
        private const string cookieName = "_XWordsAdm_Auth";

        public static User Login(string login, string password, bool isPersistent = false, HttpResponseBase response = null)
        {
            User user = null;
            using (var dbContext = new XWordsAdminModelContext())
            {
                user = string.IsNullOrEmpty(password) ? dbContext.Users.FirstOrDefault(u => u.Login.ToLower() == login.ToLower() && u.PasswordHash == string.Empty) :
                    dbContext.Users.FirstOrDefault(u => u.Login.ToLower() == login.ToLower() && u.PasswordHash == GetPasswordHash(password));
            }

            if (user == null)
                throw new AccessViolationException("Incorrect username or password");

            if (!user.Valid)
                throw new AccessViolationException("User is disable at system. Please contact support");

            if (user != null && response != null)
            {
                CreateCookie(response, login, isPersistent);
            }
            return user;
        }

        public static User GetUserById(int id, XWordsAdminModelContext dbContext)
        {
            return dbContext.Users.FirstOrDefault(u => u.Id == id);
        }

        public static void UpdatePassword(string login, string oldPassword, string newPassword)
        {
            using (var dbContext = new XWordsAdminModelContext())
            {
                var user = string.IsNullOrEmpty(oldPassword) ? dbContext.Users.FirstOrDefault(u => u.Login.ToLower() == login.ToLower() && u.PasswordHash == string.Empty) :
                    dbContext.Users.FirstOrDefault(u => u.Login.ToLower() == login.ToLower() && u.PasswordHash == GetPasswordHash(oldPassword));

                if (user == null)
                    throw new Exception("Incorrect old password");

                user.PasswordHash = string.IsNullOrEmpty(newPassword) ? string.Empty : GetPasswordHash(newPassword);
                dbContext.SaveChanges();
            }
        }

        public static void LogOut(HttpResponseBase response)
        {
            var httpCookie = response.Cookies[cookieName];
            if (httpCookie != null)
            {
                httpCookie.Value = string.Empty;
            }
        }

        public static User GetCurrentUser(HttpRequestBase request)
        {
            var authCookie = request.Cookies.Get(cookieName);
            if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                using (var dbContext = new XWordsAdminModelContext())
                {
                    return dbContext.Users.FirstOrDefault(u => u.Login.ToLower() == ticket.Name.ToLower());
                }
            }
            else
            {
                return null;
            }
        }

        public static bool IsUserLoggedIn(HttpRequestBase request)
        {
            return GetCurrentUser(request) != null;
        }

        public static string GetPasswordHash(string password)
        {
            var bytes = new UTF8Encoding().GetBytes(password);
            byte[] hashBytes;
            using (var algorithm = new System.Security.Cryptography.SHA512Managed())
            {
                hashBytes = algorithm.ComputeHash(bytes);
            }
            return Convert.ToBase64String(hashBytes);
        }

        private static void CreateCookie(HttpResponseBase response, string login, bool isPersistent = false)
        {
            var ticket = new FormsAuthenticationTicket(
                  1,
                  login,
                  DateTime.Now,
                  DateTime.Now.Add(FormsAuthentication.Timeout),
                  isPersistent,
                  string.Empty,
                  FormsAuthentication.FormsCookiePath);

            var encTicket = FormsAuthentication.Encrypt(ticket);

            var AuthCookie = new HttpCookie(cookieName)
            {
                Value = encTicket,
                Expires = DateTime.Now.Add(FormsAuthentication.Timeout)
            };
            response.Cookies.Set(AuthCookie);
        }
    }
}