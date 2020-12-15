using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace ENC
{
    public class DBOperations
    {
        private readonly DBModel db = new DBModel();
        internal string SaveSenderInformation(tbl_senderinfo tbl_Senderinfo)
        {
            try
            {
                db.tbl_senderinfo.Add(tbl_Senderinfo);
                db.SaveChanges();
                return "success";
            }
            catch(Exception ex)
            {
                return "failure" + ex.Message.ToString();
            }
        }

        internal List<SRInfo> GetSRInfo()
        {
            var query = (from sr in db.tbl_SR_allocation
                         join s in db.tbl_senderinfo on sr.sender_id equals s.sender_id
                         join r in db.tbl_receiverinfo on sr.receiver_id equals r.receiver_id
                         join d in db.tbl_datakey on sr.tbldatakey_id equals d.tbldatakey_id
                         select new
                         {
                             srid = sr.sr_id,
                             senderid = sr.sender_id,
                             sr.receiver_id,
                             datakey_id = sr.tbldatakey_id,
                             senderemail = s.sender_email,
                             receiveremail = r.receiver_email,
                             d.datafilename
                         }).Distinct().ToList();
            List<SRInfo> listSRInfo = new List<SRInfo>();
            foreach (var q in query)
            {
                SRInfo srinfo = new SRInfo
                {
                    srid = q.srid,
                    senderid = Convert.ToInt32(q.senderid),
                    receiver_id = Convert.ToInt32(q.receiver_id),
                    datakey_id = q.datakey_id,
                    senderemail = q.senderemail,
                    receiveremail = q.receiveremail,
                    datafilename = q.datafilename
                };
                listSRInfo.Add(srinfo);
            }
            return listSRInfo;
        }

       
        public List<tbl_senderinfo> GetSenderList()
        {
            return db.tbl_senderinfo.ToList();
        }

        internal void SaveFileDataToDB(tbl_datakey objtbl_datakey)
        {
            DBModel db = new DBModel();
            db.tbl_datakey.Add(objtbl_datakey);
            db.SaveChanges();
        }

        public void SaveReceiverKeyOnServer(tbl_SR_allocation objSR)
        {
            db.tbl_SR_allocation.Add(objSR);
            db.SaveChanges();
        }

        internal int GetMLength(int datakeyid)
        {
          string dkey = db.tbl_datakey.Where(x => x.tbldatakey_id == datakeyid).Select(x => x.datakey).FirstOrDefault();
          return dkey.Length;
        }

        internal string GetFileNameById(int datakeyid)
        {
            return db.tbl_datakey.Where(x => x.tbldatakey_id == datakeyid).Select(x => x.datafilename).FirstOrDefault();
        }

        internal string ValidateUserData(int sr_id, int datakeyid, string receiver_key, string sender_fname, string sender_lname, string sender_email)
        {
            if (sr_id != 0)
            {
                int sender_id = db.tbl_senderinfo.Where(x => x.sender_fname == sender_fname && x.sender_lname == sender_lname && x.sender_email == sender_email).Select(x => x.sender_id).FirstOrDefault();
                if (sender_id != 0)
                {
                    int tblsrid = db.tbl_SR_allocation.Where(x => x.sender_id == sender_id && x.receiver_key == receiver_key && x.tbldatakey_id == datakeyid).Select(x => x.sr_id).FirstOrDefault();
                    if (tblsrid != 0)
                    {
                        return "success";
                    }
                    else
                    {
                        return "failure";
                    }
                }
            }
            return "failure";
        }

        internal byte[] GetEncryptedMsg(int datakeyid)
        {
            byte[] datafile = db.tbl_datakey.Where(x => x.tbldatakey_id == datakeyid).Select(x => x.datafile).FirstOrDefault();
            return datafile;
        }

        internal string GetDataKey(int datakeyid)
        {
            string datakey = db.tbl_datakey.Where(x => x.tbldatakey_id == datakeyid).Select(x => x.datakey).FirstOrDefault();
            return datakey;
        }

        internal Dictionary<string, string> GetAllSenders()
        {
          var list =   db.tbl_senderinfo.Select(x => new { x.sender_id, x.sender_fname }).ToList();
            var dictionary = new Dictionary<string, string>();
            foreach (var l in list)
            {
                dictionary.Add(Convert.ToString(l.sender_id), l.sender_fname);
            }
            return dictionary;
        }

        internal Dictionary<string, string> GetAllReceivers()
        {
            var list = db.tbl_receiverinfo.Select(x => new { x.receiver_id, x.receiver_name }).ToList();
            var dictionary = new Dictionary<string, string>();
            foreach (var l in list)
            {
                dictionary.Add(Convert.ToString(l.receiver_id), l.receiver_name);
            }
            return dictionary;
        }

        internal string GetServiceType(int datakeyid)
        {
            byte[] buffer = db.tbl_datakey.Where(x => x.tbldatakey_id == datakeyid).Select(x => x.datafile).FirstOrDefault();
            string serviceType = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            return serviceType;
        }

        internal Dictionary<string, string> GetAllDataFileIds(string senderid)
        {
            int sid = Convert.ToInt32(senderid);
            var list = db.tbl_datakey.Where(x=>x.sender_id == sid).Select(x => new { x.tbldatakey_id, x.datafilename, x.datafile }).ToList();
            
            var dictionary = new Dictionary<string, string>();
            foreach (var l in list)
            {
                string serviceType = Encoding.UTF8.GetString(l.datafile);
                if (serviceType.Equals("A")){
                    dictionary.Add(Convert.ToString(l.tbldatakey_id), l.datafilename + " in Amazon");
                }
                else if (serviceType.Equals("Z"))
                {
                    dictionary.Add(Convert.ToString(l.tbldatakey_id), l.datafilename  +" in Azure");
                }
                else if (serviceType.Equals("C"))
                {
                    dictionary.Add(Convert.ToString(l.tbldatakey_id), l.datafilename + " in Okeanos");
                }
            }
            return dictionary;
        }
    }
}