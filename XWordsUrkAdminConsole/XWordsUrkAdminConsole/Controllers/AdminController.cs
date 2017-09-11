using XWordsUrkAdminConsole.Models;
using System.Web.Mvc;
using System.Linq;
using DataTables.Mvc;
using System.Linq.Dynamic;
using System;
using System.Collections.Generic;

namespace XWordsUrkAdminConsole.Controllers
{
    public class AdminController : Controller
    {
        private XWordsAdminModelContext _dbContext = new XWordsAdminModelContext();

        public AdminController()
        {
        }

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetWords([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            IQueryable<Word> query = _dbContext.Words;
            var totalCount = query.Count();

            // Apply filters for searching
            if (requestModel.Search.Value != string.Empty)
            {
                var value = requestModel.Search.Value.Trim();
                query = query.Where(p => p.TheWord.Contains(value) ||
                                         p.Definition.Contains(value));
            }

            var filteredCount = query.Count();

            // Sorting
            var sortedColumns = requestModel.Columns.GetSortedColumns();
            var orderByString = string.Empty;
            
            foreach (var column in sortedColumns)
            {
                orderByString += orderByString != string.Empty ? "," : "";
                orderByString += (column.Data) +
                  (column.SortDirection ==
                  Column.OrderDirection.Ascendant ? " asc" : " desc");
            }
            orderByString.Replace("AreaView", "Area");

            query = query.OrderBy(orderByString == string.Empty ? "TheWord asc" : orderByString);

            // Paging
            query = query.Skip(requestModel.Start).Take(requestModel.Length);

            var data = query.Select(word => new
            {
                Id = word.Id,
                TheWord = word.TheWord,
                Definition = word.Definition,
                Area = word.Area,
                Complexity = word.Complexity,
                State = word.State,
                LastModified = word.LastModified
            }).ToList();
            
            return Json(new DataTablesResponse(requestModel.Draw, data, filteredCount, totalCount),
                 JsonRequestBehavior.AllowGet);            
        }

        public ActionResult GetWordAreas()
        {
            var res = new Dictionary<string, string>();
            foreach (var e in Enum.GetValues(typeof(WordArea)))
            {
                res.Add(((int)e).ToString(), e.ToString());
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWordComplexities()
        {
            var res = new Dictionary<string, string>();
            foreach (var e in Enum.GetValues(typeof(WordComplexity)))
            {
                res.Add(((int)e).ToString(), e.ToString());
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWordStates()
        {
            var res = new Dictionary<string, string>();
            foreach (var e in Enum.GetValues(typeof(WordState)))
            {
                res.Add(((int)e).ToString(), e.ToString());
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Words()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_dbContext != null)
                _dbContext.Dispose();
        }
    }
}