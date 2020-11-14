using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ENC
{
    public class SRInfo
    {
        [Key]
        public int srid { get; set; }              
        public int senderid { get; set; }
        public int receiver_id { get; set; }
        public int datakey_id { get; set; }
        public string senderemail { get; set; }
        public string receiveremail { get; set; }
        public string datafilename { get; set; }
    }                                 
}