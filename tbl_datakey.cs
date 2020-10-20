namespace ENC
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_datakey
    {
        [Key]
        public int tbldatakey_id { get; set; }

        public int? sender_id { get; set; }

        [StringLength(50)]
        public string datakey { get; set; }
        
        [StringLength(50)]
        public string datafilename { get; set; }
        public byte[] datafile { get; set; }


    }
}
