﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class XWordsAdminModelContext : DbContext
    {
        public XWordsAdminModelContext()
            : base("name=XWordsAdminModelContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Word> Words { get; set; }
        public virtual DbSet<Clue> Clues { get; set; }
        public virtual DbSet<Media> Medias { get; set; }
        public virtual DbSet<Game> Games { get; set; }
        public virtual DbSet<Setting> Settings { get; set; }
        public virtual DbSet<Version> Versions { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Event> Events { get; set; }
    }
}
