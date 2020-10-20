namespace ENC
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_receiverinfo
    {
        [Key]
        public int receiver_id { get; set; }

        [StringLength(50)]
        public string receiver_name { get; set; }

        [StringLength(35)]
        public string receiver_email { get; set; }
    }
}
