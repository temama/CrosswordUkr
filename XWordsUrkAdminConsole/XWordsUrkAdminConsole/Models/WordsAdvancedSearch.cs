using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XWordsUrkAdminConsole.Models
{
    public class WordsAdvancedSearch
    {
        public bool ShowDeleted { get; set; }
        public string AreasFilter { get; set; }
        public string ComplexityFilter { get; set; }
        public string StatesFilter { get; set; }
        public string ModifiedByFilter { get; set; }
        public string ModifiedFrom { get; set; }
        public string ModifiedTo { get; set; }
    }
}