namespace ENC
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DBModel : DbContext
    {
        public DBModel()
            : base("name=DBModel")
        {
        }

        public virtual DbSet<tbl_senderinfo> tbl_senderinfo { get; set; }
        public virtual DbSet<tbl_datakey> tbl_datakey { get; set; }

        public virtual DbSet<tbl_receiverinfo> tbl_receiverinfo { get; set; }
        public virtual DbSet<tbl_SR_allocation> tbl_SR_allocation { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
