using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using DataTables.Mvc;
using XWordsUrkAdminConsole.Accounting;
using XWordsUrkAdminConsole.Models;
using System.Data.Entity;

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

        public ActionResult GetGameTypes()
        {
            var res = new Dictionary<string, string>();
            foreach (var e in Enum.GetValues(typeof(GameType)))
            {
                res.Add(((int)e).ToString(), e.ToString());
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGameComplexities()
        {
            var res = new Dictionary<string, string>();
            foreach (var e in Enum.GetValues(typeof(GameComplexity)))
            {
                res.Add(((int)e).ToString(), e.ToString());
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGameStates()
        {
            var res = new Dictionary<string, string>();
            foreach (var e in Enum.GetValues(typeof(GameState)))
            {
                res.Add(((int)e).ToString(), e.ToString());
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGames([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, GamesAdvancedSearch advSearch)
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return Json(new
                {
                    IsRedirect = true,
                    Message = " Please login to proceed",
                    RedirectUrl = Url.Action("Login", "Home")
                }, JsonRequestBehavior.AllowGet);

            using (var dbContext = new XWordsAdminModelContext())
            {
                IQueryable<Game> query = dbContext.Games;
                var totalCount = query.Count();

                // Apply filters for searching
                if (!string.IsNullOrEmpty(requestModel.Search.Value))
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(g => g.Description.Contains(value));
                }

                if (!string.IsNullOrEmpty(advSearch.GameTypesFilter))
                {
                    var opts = advSearch.GameTypesFilter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var optsEnum = opts.Select(o => (GameType)Convert.ToUInt32(o)).ToList();
                    query = query.Where(g => optsEnum.Contains(g.Type));
                }

                if (!string.IsNullOrEmpty(advSearch.ComplexityFilter))
                {
                    var opts = advSearch.ComplexityFilter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var optsEnum = opts.Select(o => (GameComplexity)Convert.ToUInt32(o)).ToList();
                    query = query.Where(g => optsEnum.Contains(g.Complexity));
                }

                if (!string.IsNullOrEmpty(advSearch.StatesFilter))
                {
                    if (advSearch.ShowRejected && !advSearch.StatesFilter.Contains(string.Format("{0},", (int)GameState.Rejected)))
                        advSearch.StatesFilter += (int)WordState.Deleted;
                    var opts = advSearch.StatesFilter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var optsEnum = opts.Select(o => (GameState)Convert.ToUInt32(o)).ToList();
                    query = query.Where(g => optsEnum.Contains(g.State));
                }
                else if (!advSearch.ShowRejected)
                {
                    query = query.Where(g => g.State != GameState.Rejected);
                }

                if (!string.IsNullOrEmpty(advSearch.ModifiedByFilter))
                {
                    var opts = advSearch.ModifiedByFilter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => Convert.ToInt32(s)).ToList();
                    query = query.Where(g => opts.Contains(g.UserId));
                }

                var filteredCount = query.Count();

                // Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;

                foreach (var column in sortedColumns)
                {
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += GetGameSortColumnName(column.Data) +
                      (column.SortDirection ==
                      Column.OrderDirection.Ascendant ? " asc" : " desc");
                }

                query = query.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);

                // Paging
                query = query.Skip(requestModel.Start).Take(requestModel.Length);

                var data = query.Include("Users").Select(g => new
                {
                    Id = g.Id,
                    GameType = g.Type,
                    Description = g.Description,
                    Complexity = g.Complexity,
                    IncludedFromVer = g.IncludedFromVer,
                    ExcludedFromVer = g.ExcludedFromVer,
                    State = g.State,
                    LastModified = g.LastModified,
                    ModifiedBy = g.ModifiedBy.Initials
                }).ToList();

                return Json(new DataTablesResponse(requestModel.Draw, data, filteredCount, totalCount),
                     JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult NewGameMenu()
        {
            return PartialView("NewGameMenu");
        }

        private string GetGameSortColumnName(string dataName)
        {
            if (dataName == "ModifiedBy")
                return "ModifiedBy.Initials";

            return dataName;
        }
    }
}