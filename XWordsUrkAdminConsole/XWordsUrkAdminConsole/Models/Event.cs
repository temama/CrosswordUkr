//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace XWordsUrkAdminConsole.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Event
    {
        public int Id { get; set; }
        public System.DateTime TimeStamp { get; set; }
        public int UserId { get; set; }
        public string Table { get; set; }
        public int RecordId { get; set; }
        public string Comment { get; set; }
    
        public virtual User User { get; set; }
    }
}
