using XWordsUrkAdminConsole.Models;
using System.Web.Mvc;
using System.Linq;
using DataTables.Mvc;
using System.Linq.Dynamic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Entity;

namespace XWordsUrkAdminConsole.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetWords([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, WordsAdvancedSearch advSearch)
        {
            using (var dbContext = new XWordsAdminModelContext())
            {
                IQueryable<Word> query = dbContext.Words;
                var totalCount = query.Count();

                // Apply filters for searching
                if (requestModel.Search.Value != string.Empty)
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(p => p.TheWord.Contains(value) ||
                                             p.Definition.Contains(value));
                }
                                
                if (!string.IsNullOrEmpty(advSearch.AreasFilter))
                {
                    var opts = advSearch.AreasFilter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var optsEnum = opts.Select(o => (WordArea)Convert.ToUInt32(o)).ToList();
                    query = query.Where(w => optsEnum.Contains(w.Area));
                }

                if (!string.IsNullOrEmpty(advSearch.ComplexityFilter))
                {
                    var opts = advSearch.ComplexityFilter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var optsEnum = opts.Select(o => (WordComplexity)Convert.ToUInt32(o)).ToList();
                    query = query.Where(w => optsEnum.Contains(w.Complexity));
                }

                if (!string.IsNullOrEmpty(advSearch.StatesFilter))
                {
                    if (advSearch.ShowDeleted && !advSearch.StatesFilter.Contains(string.Format("{0},", (int)WordState.Deleted)))
                        advSearch.StatesFilter += (int)WordState.Deleted;
                    var opts = advSearch.StatesFilter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var optsEnum = opts.Select(o => (WordComplexity)Convert.ToUInt32(o)).ToList();
                    query = query.Where(w => optsEnum.Contains(w.Complexity));
                }
                else if (!advSearch.ShowDeleted)
                {
                    query = query.Where(w => w.State != WordState.Deleted);
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

        public ActionResult WordDetails(int? id)
        {
            using (var dbContext = new XWordsAdminModelContext())
            {
                if (id == null || id < 0)
                {
                    var user = dbContext.Users.First(u => u.Id == 2); //HARDCOOOODE
                    return PartialView("WordDetails", new Word()
                    {
                        Id = -1,
                        LastModified = DateTime.Now,
                        UserId = user.Id,
                        ModifiedBy = user
                    });
                }

                Word word = null;

                word = dbContext.Words.Include(w => w.ModifiedBy).FirstOrDefault(w => w.Id == id);

                return PartialView("WordDetails", word);
            }
        }

        [HttpPost]
        public JsonResult SaveWord(Word postWord)
        {
            // TODO: check permissions
            // TODO: save under correct user (ModifiedBy)
            try
            {
                Word word;

                using (var dbContext = new XWordsAdminModelContext())
                {
                    var eventComment = new StringBuilder();
                    var user = dbContext.Users.First(u => u.Id == 2); //HARDCOOOODE

                    if (postWord.Id <= 0)
                    {
                        // TODO: Check if word exist
                        word = new Word();
                        dbContext.Words.Add(word);
                        eventComment.Append("added new");
                    }
                    else
                    {
                        word = dbContext.Words.Include(w => w.ModifiedBy).First(w => w.Id == postWord.Id);
                        eventComment.Append("updated");
                    }

                    var timestamp = DateTime.Now;

                    word.TheWord = postWord.TheWord.ToUpper();
                    word.Definition = postWord.Definition;
                    word.Area = postWord.Area;
                    word.Complexity = postWord.Complexity;
                    word.State = postWord.State;
                    word.LastModified = timestamp;
                    word.UserId = user.Id; 
                    word.ModifiedBy = user;

                    dbContext.SaveChanges();

                    var audit = new Event()
                    {
                        TimeStamp = timestamp,
                        UserId = 2,
                        Table = "Words",
                        RecordId = word.Id,
                        Comment = eventComment.ToString()
                    };
                    dbContext.Events.Add(audit);
                    dbContext.SaveChanges();

                    return Json(new
                    {
                        Id = word.Id,
                        TheWord = word.TheWord,
                        Definition = word.Definition,
                        Area = word.Area,
                        Complexity = word.Complexity,
                        State = word.State,
                        LastModified = word.LastModified,
                        UserId = word.UserId
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetClues([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, CluesAdvancedSearch advSearch)
        {
            using (var dbContext = new XWordsAdminModelContext())
            {
                IQueryable<Clue> query = dbContext.Clues;
                var totalCount = query.Count();

                if (advSearch.WordId >= 0)
                {
                    query = query.Where(c => c.WordId == advSearch.WordId);
                }

                // Apply filters for searching
                if (requestModel.Search.Value != string.Empty)
                {
                    var value = requestModel.Search.Value.Trim();
                    query = query.Where(c => c.Word.TheWord.Contains(value) ||
                                             c.TheClue.Contains(value));
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

                query = query.OrderBy(orderByString == string.Empty ? "Word asc" : orderByString);

                // Paging
                query = query.Skip(requestModel.Start).Take(requestModel.Length);

                // Fetching
                var data = query.Select(clue => new
                {
                    Id = clue.Id,
                    WordId = clue.WordId,
                    Word = clue.Word,
                    TheClue = clue.TheClue,
                    Complexity = clue.Complexity,
                    State = clue.State,
                    IncludedFromVer = clue.IncludedFromVer,
                    ExcludedFromVer = clue.ExcludedFromVer,
                    LastModified = clue.LastModified,
                    ModifiedBy = clue.ModifiedBy.Initials
                }).ToList();

                return Json(new DataTablesResponse(requestModel.Draw, data, filteredCount, totalCount),
                     JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}