using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ENC
{
    public class DownloadDataModel
    {
        public int datakey_id { get; set; }
        public int sr_id { get; set; }
        public string datafilename { get; set; }
        public string rkey { get; set; }
        public string sfname { get; set; }
       
        public string slname { get; set; }
        public string semail { get; set; }

        public string stype { get; set; }
    }

    public class ListDownloadDataModel
    {
        public List<DownloadDataModel> listdownloadDataModel { get; set; }
    }
}