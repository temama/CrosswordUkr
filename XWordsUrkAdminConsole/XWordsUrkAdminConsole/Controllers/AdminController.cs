using XWordsUrkAdminConsole.Models;
using System.Web.Mvc;
using System.Linq;
using DataTables.Mvc;
using System.Linq.Dynamic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Entity;
using XWordsUrkAdminConsole.Helpers;
using XWordsUrkAdminConsole.Accounting;

namespace XWordsUrkAdminConsole.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return RedirectToAction("Login", "Home", new { ReturnUrl = Request.RawUrl });

            return View();
        }

        public ActionResult GetWords([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, WordsAdvancedSearch advSearch)
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
                    var optsEnum = opts.Select(o => (WordState)Convert.ToUInt32(o)).ToList();
                    query = query.Where(w => optsEnum.Contains(w.State));
                }
                else if (!advSearch.ShowDeleted)
                {
                    query = query.Where(w => w.State != WordState.Deleted);
                }

                if (!string.IsNullOrEmpty(advSearch.ModifiedByFilter))
                {
                    var opts = advSearch.ModifiedByFilter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => Convert.ToInt32(s)).ToList();
                    query = query.Where(w => opts.Contains(w.UserId));
                }

                var filteredCount = query.Count();

                // Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;

                foreach (var column in sortedColumns)
                {
                    orderByString += orderByString != string.Empty ? "," : "";
                    orderByString += GetWordSortColumnName(column.Data) +
                      (column.SortDirection ==
                      Column.OrderDirection.Ascendant ? " asc" : " desc");
                }

                query = query.OrderBy(orderByString == string.Empty ? "TheWord asc" : orderByString);

                // Paging
                query = query.Skip(requestModel.Start).Take(requestModel.Length);

                var data = query.Include("Users").Select(word => new
                {
                    Id = word.Id,
                    TheWord = word.TheWord,
                    Definition = word.Definition,
                    Area = word.Area,
                    Complexity = word.Complexity,
                    State = word.State,
                    LastModified = word.LastModified,
                    ModifiedBy = word.ModifiedBy.Initials
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

        public ActionResult GetClueStates()
        {
            var res = new Dictionary<string, string>();
            foreach (var e in Enum.GetValues(typeof(ClueState)))
            {
                res.Add(((int)e).ToString(), e.ToString());
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Words()
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return RedirectToAction("Login", "Home", new { ReturnUrl = Request.RawUrl });

            return View("Words");
        }

        public ActionResult GoToWord(int? id)
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return RedirectToAction("Login", "Home", new { ReturnUrl = Request.RawUrl });

            return View("Words", id);
        }

        public ActionResult WordDetails(int? id)
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return RedirectToAction("Login", "Home", new { ReturnUrl = Request.RawUrl });
            //return View("../Home/Login", new LoginViewModel() { ReturnUrl = Request.RawUrl });

            if (id == null || id < 0)
            {
                return PartialView("WordDetails", new Word()
                {
                    Id = -1,
                    LastModified = DateTime.Now,
                    UserId = reqUser.Id,
                    ModifiedBy = reqUser
                });
            }

            using (var dbContext = new XWordsAdminModelContext())
            {
                Word word = dbContext.Words.Include(w => w.ModifiedBy).FirstOrDefault(w => w.Id == id);

                return PartialView("WordDetails", word);
            }
        }

        [HttpPost]
        public JsonResult SaveWord(Word postWord)
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return Json(new
                {
                    IsRedirect = true,
                    Message = " Please login to proceed",
                    RedirectUrl = Url.Action("Login", "Home")
                }, JsonRequestBehavior.AllowGet);
            // TODO: check permissions??
            try
            {
                Word word;
                string theWord = postWord.TheWord.ToUpper().Trim();

                using (var dbContext = new XWordsAdminModelContext())
                {
                    var eventComment = new StringBuilder();

                    if (postWord.Id <= 0)
                    {
                        if (dbContext.Words.Any(w => w.TheWord == theWord))
                            throw new Exception(string.Format("Word '{0}' is already exists", theWord));

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

                    word.TheWord = theWord;
                    word.Definition = postWord.Definition;
                    word.Area = postWord.Area;
                    word.Complexity = postWord.Complexity;
                    word.State = postWord.State;
                    word.LastModified = timestamp;
                    word.UserId = reqUser.Id; 
                    word.ModifiedBy = AuthModule.GetUserById(reqUser.Id, dbContext);
                    
                    dbContext.SaveChanges();

                    var audit = new Event()
                    {
                        TimeStamp = timestamp,
                        UserId = reqUser.Id,
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
                return Json("ERROR: " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult WordLookup(string searchQuery)
        {
            using (var dbContext = new XWordsAdminModelContext())
            {
                var query = dbContext.Words.Where(w => w.TheWord.Contains(searchQuery.ToUpper()));
                var res = query.Take(10).Select(w => new
                {
                    Id = w.Id,
                    Word = w.TheWord,
                    Description = w.Definition,
                    Compl = (int)w.Complexity
                }).ToList();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Clues()
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return RedirectToAction("Login", "Home", new { ReturnUrl = Request.RawUrl });

            return View("Clues");
        }

        public ActionResult GoToClue(int? id)
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return RedirectToAction("Login", "Home", new { ReturnUrl = Request.RawUrl });

            return View("Clues", id);
        }

        public ActionResult GetClues([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, CluesAdvancedSearch advSearch)
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
                IQueryable<Clue> query = dbContext.Clues;
                var totalCount = query.Count();

                if (advSearch.WordId > 0)
                {
                    query = query.Where(c => c.WordId == advSearch.WordId);
                }

                if (!string.IsNullOrEmpty(advSearch.ComplexityFilter))
                {
                    var opts = advSearch.ComplexityFilter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var optsEnum = opts.Select(o => (WordComplexity)Convert.ToUInt32(o)).ToList();
                    query = query.Where(c => optsEnum.Contains(c.Complexity));
                }

                if (!string.IsNullOrEmpty(advSearch.StatesFilter))
                {
                    if (advSearch.ShowRejected && !advSearch.StatesFilter.Contains(string.Format("{0},", (int)ClueState.Rejected)))
                        advSearch.StatesFilter += (int)ClueState.Rejected;
                    var opts = advSearch.StatesFilter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var optsEnum = opts.Select(o => (ClueState)Convert.ToUInt32(o)).ToList();
                    query = query.Where(c => optsEnum.Contains(c.State));
                }
                else if (!advSearch.ShowRejected)
                {
                    query = query.Where(w => w.State != ClueState.Rejected);
                }

                if (!string.IsNullOrEmpty(advSearch.ModifiedByFilter))
                {
                    var opts = advSearch.ModifiedByFilter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => Convert.ToInt32(s)).ToList();
                    query = query.Where(w => opts.Contains(w.UserId));
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
                    Word = clue.Word.TheWord,
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

        public ActionResult ClueDetails(int? id)
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return RedirectToAction("Login", "Home", new { ReturnUrl = Request.RawUrl });
            
            if (id == null || id < 0)
            {
                return PartialView("ClueDetails", new Clue()
                {
                    Id = -1,
                    LastModified = DateTime.Now,
                    UserId = reqUser.Id,
                    ModifiedBy = reqUser
                });
            }

            using (var dbContext = new XWordsAdminModelContext())
            {
                Clue clue = dbContext.Clues.Include(c => c.ModifiedBy).Include(c => c.Word)
                    .FirstOrDefault(c => c.Id == id);

                return PartialView("ClueDetails", clue);
            }
        }

        [HttpPost]
        public ActionResult NewClueForWord(int wordId)
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return RedirectToAction("Login", "Home", new { ReturnUrl = Request.RawUrl });

            using (var dbContext = new XWordsAdminModelContext())
            {
                var word = dbContext.Words.FirstOrDefault(w => w.Id == wordId);
                var clue = new Clue()
                {
                    Id = -1,
                    WordId = word.Id,
                    Word = word,
                    GameType = ClueGameType.Crossword,
                    Complexity = word.Complexity,
                    State = ClueState.New,
                    // TODO: IncludedFromVer = <Populate by current UAT version>
                    LastModified = DateTime.Now,
                    UserId = reqUser.Id,
                    ModifiedBy = reqUser
                };

                return PartialView("ClueDetails", clue);
            }
        }

        [HttpPost]
        public JsonResult SaveClue(Clue postClue)
        {
            var reqUser = AuthModule.GetCurrentUser(Request, true, Response);
            if (reqUser == null)
                return Json(new
                {
                    IsRedirect = true,
                    Message = " Please login to proceed",
                    RedirectUrl = Url.Action("Login", "Home")
                }, JsonRequestBehavior.AllowGet);
            // TODO: check permissions??
            try
            {
                Clue clue;
                using (var dbContext = new XWordsAdminModelContext())
                {
                    var eventComment = new StringBuilder();

                    if (postClue.Id <= 0)
                    {
                        clue = new Clue();
                        dbContext.Clues.Add(clue);
                        eventComment.Append("added new");
                    }
                    else
                    {
                        clue = dbContext.Clues.Include(w => w.ModifiedBy).First(c => c.Id == postClue.Id);
                        eventComment.Append("updated");
                    }

                    var timestamp = DateTime.Now;

                    clue.WordId = postClue.WordId;
                    clue.GameType = postClue.GameType;
                    clue.TheClue = postClue.TheClue;
                    clue.State = postClue.State;
                    clue.Complexity = postClue.Complexity;
                    clue.IncludedFromVer = postClue.IncludedFromVer;
                    clue.ExcludedFromVer = postClue.ExcludedFromVer;
                    clue.LastModified = timestamp;
                    clue.UserId = reqUser.Id;
                    clue.ModifiedBy = AuthModule.GetUserById(reqUser.Id, dbContext);

                    dbContext.SaveChanges();

                    var audit = new Event()
                    {
                        TimeStamp = timestamp,
                        UserId = reqUser.Id,
                        Table = "Clues",
                        RecordId = clue.Id,
                        Comment = eventComment.ToString()
                    };
                    dbContext.Events.Add(audit);
                    dbContext.SaveChanges();

                    return Json(new
                    {
                        Id = clue.Id,
                        WordId = clue.WordId,
                        GameType = clue.GameType,
                        TheClue = clue.TheClue,
                        State = clue.State,
                        Complexity = clue.Complexity,
                        IncludedFromVer = clue.IncludedFromVer,
                        ExcludedFromVer = clue.ExcludedFromVer,
                        UserId = clue.UserId
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("ERROR: " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private string GetWordSortColumnName(string dataName)
        {
            if (dataName == "ModifiedBy")
                return "ModifiedBy.Initials";

            return dataName;
        }
    }
}