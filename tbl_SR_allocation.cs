namespace ENC
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    public partial class tbl_SR_allocation
    {
        DataOperations d = new DataOperations();

        [Key]
        public int sr_id { get; set; }

        public int? sender_id { get; set; }

        public int? receiver_id { get; set; }

        public int tbldatakey_id { get; set; }

        [StringLength(50)]
        public string receiver_key { get; set; }

        //public List<SelectListItem> Senders
        //{
        //    get
        //    {
        //       return d.getSenderList();
        //    }
            
        //}
    }
}
