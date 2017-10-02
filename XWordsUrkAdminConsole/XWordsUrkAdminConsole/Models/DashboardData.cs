using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XWordsUrkAdminConsole.Models
{
    public class DashboardData
    {
        public int TotalWords { get; set; }
        public int TotalClues { get; set; }
        public int TotalGames { get; set; }
        public int WordsProcessed { get; set; }
        public Dictionary<WordState, int> WordsStates { get; set; }
        public Dictionary<Clue, int> CluesStates { get; set; }
    }
}