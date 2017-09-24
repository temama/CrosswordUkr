﻿using XWordsUrkAdminConsole.Models;
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
            var reqUser = AuthModule.GetCurrentUser(Request);
            if (reqUser == null)
                return RedirectToAction("Login", "Home", new { ReturnUrl = Request.RawUrl });

            return View();
        }

        public ActionResult GetWords([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, WordsAdvancedSearch advSearch)
        {
            var reqUser = AuthModule.GetCurrentUser(Request);
            if (reqUser == null)
                return Json("Please login to proceed");

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
            var reqUser = AuthModule.GetCurrentUser(Request);
            if (reqUser == null)
                return RedirectToAction("Login", "Home", new { ReturnUrl = Request.RawUrl });

            return View();
        }

        public ActionResult WordDetails(int? id)
        {
            var reqUser = AuthModule.GetCurrentUser(Request);
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
            var reqUser = AuthModule.GetCurrentUser(Request);
            if (reqUser == null)
                return Json("ERROR: Please login to proceed");
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

        public ActionResult GetClues([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, CluesAdvancedSearch advSearch)
        {
            var reqUser = AuthModule.GetCurrentUser(Request);
            if (reqUser == null)
                return Json("Please login to work with Clues");

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

        public ActionResult ClueDetails(int? id)
        {
            var reqUser = AuthModule.GetCurrentUser(Request);
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
                Clue clue = dbContext.Clues.Include(c => c.ModifiedBy).FirstOrDefault(c => c.Id == id);

                return PartialView("ClueDetails", clue);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}