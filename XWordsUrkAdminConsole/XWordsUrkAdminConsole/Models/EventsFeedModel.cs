using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XWordsUrkAdminConsole.Models
{
    public class EventsFeedModel
    {
        public User User { get; set; }
        public string TimeAgoRepresentation { get; set; }
        public string IconClass { get; set; }
        public string EventRepresentation { get; set; }
    }
}