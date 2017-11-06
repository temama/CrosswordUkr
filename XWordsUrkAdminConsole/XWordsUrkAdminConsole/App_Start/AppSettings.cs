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

                LoadSettings(dbContext);
            }
        }

        private static void LoadSettings(XWordsAdminModelContext dbContext)
        {
            foreach (var setting in dbContext.Settings)
            {
                All[setting.Name] = setting.Value;

                if (setting.Name == "CurrentTestingVersion")
                {
                    int id = Convert.ToInt32(setting.Value);
                    CurrentTestingVersion = dbContext.Versions.FirstOrDefault(v => v.Id == id);
                }
                if (setting.Name == "CurrentLiveVersion")
                {
                    int id = Convert.ToInt32(setting.Value);
                    CurrentLiveVersion = dbContext.Versions.FirstOrDefault(v => v.Id == id);
                }
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