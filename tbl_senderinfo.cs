namespace ENC
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_senderinfo
    {
        [Key]
        public int sender_id { get; set; }

        [StringLength(20)]
        public string sender_fname { get; set; }

        [StringLength(20)]
        public string sender_lname { get; set; }

        [StringLength(50)]
        public string sender_email { get; set; }

        [StringLength(20)]
        public string secret_word { get; set; }

        [StringLength(100)]
        public string sender_attribute { get; set; }
    }
}
