using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XWordsUrkAdminConsole.Models;

namespace XWordsUrkAdminConsole
{
    public class AppSettings
    {
        public static User SystemUser { get; private set; }

        public static Models.Version CurrentTestingVersion { get; private set; }
        public static Models.Version CurrentLiveVersion { get; private set; }
        public static Dictionary<string, string> All { get; private set; }

        public static void Init()
        {
            All = new Dictionary<string, string>();
            using (var dbContext = new XWordsAdminModelContext())
            {
                LoadSystemUser(dbContext);
            }
        }

        private static void LoadSystemUser(XWordsAdminModelContext dbContext)
        {
            SystemUser = dbContext.Users.FirstOrDefault(u=>u.Login == "System");
            if (SystemUser == null)
                throw new Exception("System user is not defined at DB. App can't start");
        }
    }
}