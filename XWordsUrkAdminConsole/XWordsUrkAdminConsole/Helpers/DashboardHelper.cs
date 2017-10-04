using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Data.Entity;
using XWordsUrkAdminConsole.Models;

namespace XWordsUrkAdminConsole.Helpers
{
    public class DashboardHelper
    {
        private static List<string> _modelTables = new List<string> { "Words", "Clues", "Games", "Media",
            "Versions", "Users", "Settings"};
        private static string _iconAdd = "glyphicon glyphicon-plus-sign";
        private static string _iconEdit = "glyphicon glyphicon-edit";
        private static string _iconCalendar = "glyphicon glyphicon-calendar";

        public static string GetTimeAgoRepresentation(DateTime when)
        {
            var ts = DateTime.Now - when;
            switch (ts.Days)
            {
                case 0:
                    if (ts.Hours < 1)
                        return ts.Minutes + " minutes ago";
                    else
                        return ts.Hours + " hours ago";
                case 1:
                    return "Yesterday, at " + when.ToString("HH:mm");
                case 2:
                case 3:
                case 4:
                    return "On " + when.DayOfWeek.ToString() + ", at " + when.ToString("HH:mm");
                default:
                    return ts.Days + " days ago";
            }

            //return "Once";
        }

        public static IEnumerable<EventsFeedModel> GetEventsFeed(int count, int? skipFirstN)
        {
            var skip = 0;
            if (skipFirstN != null && skipFirstN.HasValue)
                skip = skipFirstN.Value;

            var res = new List<EventsFeedModel>();
            using (var dbContext = new XWordsAdminModelContext())
            {
                var query = dbContext.Events.Include(e => e.User).OrderByDescending(e => e.TimeStamp).Skip(skip).Take(count);
                foreach (var e in query)
                {
                    res.Add(GetEventsFeedItem(e, dbContext));
                }
            }
            return res;
        }

        private static EventsFeedModel GetEventsFeedItem(Event e, XWordsAdminModelContext dbContext)
        {
            var res = new EventsFeedModel
            {
                User = e.User,
                TimeAgoRepresentation = GetTimeAgoRepresentation(e.TimeStamp)
            };

            if (_modelTables.Contains(e.Table))
            {
                if (e.Comment.ToLower().StartsWith("added new"))
                    res.IconClass = _iconAdd;
                else
                    res.IconClass = _iconEdit;
            }
            else
            {
                res.IconClass = _iconCalendar;
            }

            res.EventRepresentation = GenerateEventRepresentation(e, dbContext);

            return res;
        }
        
        private static string GenerateEventRepresentation(Event e, XWordsAdminModelContext dbContext)
        {
            return e.Comment + " " + e.Table;
        }
    }
}