using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Data.Entity;
using XWordsUrkAdminConsole.Models;
using System.Text;
using System.Web.Mvc;

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

        public static IEnumerable<EventsFeedModel> GetEventsFeed(UrlHelper Url, int count, int? skipFirstN)
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
                    res.Add(GetEventsFeedItem(Url, e, dbContext));
                }
            }
            return res;
        }

        public static string GetVeryShortDayRepr(DayOfWeek d)
        {
            switch (d)
            {
                case DayOfWeek.Sunday:
                    return "Su";
                case DayOfWeek.Monday:
                    return "M";
                case DayOfWeek.Tuesday:
                    return "Tu";
                case DayOfWeek.Wednesday:
                    return "W";
                case DayOfWeek.Thursday:
                    return "Th";
                case DayOfWeek.Friday:
                    return "F";
                case DayOfWeek.Saturday:
                    return "Sa";
                default:
                    return "";
            }
        }

        private static EventsFeedModel GetEventsFeedItem(UrlHelper Url, Event e, XWordsAdminModelContext dbContext)
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

            res.EventRepresentation = GenerateEventRepresentation(Url, e, dbContext);

            return res;
        }
        
        private static string GenerateEventRepresentation(UrlHelper Url, Event e, XWordsAdminModelContext dbContext)
        {
            var res = new StringBuilder();

            if (e.Table == "Words")
            {
                res.Append(e.Comment.ToLower().StartsWith("added new") ? "Added new " : "Updated ");
                res.Append(" word ");

                var word = dbContext.Words.First(w => w.Id == e.RecordId);
                res.Append(string.Format("<a href=\"{0}\">{1}</a>", Url.Action("GoToWordDetails", "Admin") + "/" + word.Id, word.TheWord));
            }
            else if (e.Table == "Clues")
            {
                res.Append(e.Comment.ToLower().StartsWith("added new") ? "Added new " : "Updated ");
                res.Append(" clue ");

                var clue = dbContext.Clues.First(c => c.Id == e.RecordId);
                res.Append(string.Format("<a href=\"{0}\">{1}</a>", Url.Action("GoToClueDetails", "Admin") + "/" + clue.Id, CutText(clue.TheClue, 10)));
                res.Append(" for word ");
                res.Append(string.Format("<a href=\"{0}\">{1}</a>", Url.Action("GoToWordDetails", "Admin") + "/" + clue.Word.Id, clue.Word.TheWord));
            }

            return res.ToString();
        }

        private static string CutText(string text, int len)
        {
            if (text.Length <= len)
                return text;

            return text.Substring(0, len) + "...";
        }
    }
}