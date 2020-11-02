using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace ENC
{
    public class DBOperations
    {
        private DBModel db = new DBModel();
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
                return "failure";
            }
        }

        internal void SaveDataKeyOnServer(int senderId, string datakey)
        {
        //    DBModel db = new DBModel();
        //    tbl_datakey objtbl_Datakey = new tbl_datakey();
        //    objtbl_Datakey.sender_id = senderId;
        //    objtbl_Datakey.datakey = datakey;
        //    db.tbl_datakey.Add(objtbl_Datakey);
        //    db.SaveChanges();
        }

        public List<tbl_senderinfo> getSenderList()
        {
            return db.tbl_senderinfo.ToList();
        }

        internal void saveFileDataToDB(tbl_datakey objtbl_datakey)
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

        internal int getMLength(int datakeyid)
        {
          string dkey = db.tbl_datakey.Where(x => x.tbldatakey_id == datakeyid).Select(x => x.datakey).FirstOrDefault();
          return dkey.Length;
        }

        internal string validateUserData(int sr_id, int datakeyid, string receiver_key, string sender_fname, string sender_lname, string sender_email)
        {
            int sender_id = db.tbl_senderinfo.Where(x=>x.sender_fname == sender_fname && x.sender_lname == sender_lname && x.sender_email == sender_email).Select(x=>x.sender_id).FirstOrDefault();
            if(sender_id != 0)
            {
                int tblsrid = db.tbl_SR_allocation.Where(x => x.sender_id == sender_id && x.receiver_key == receiver_key && x.tbldatakey_id == datakeyid).Select(x => x.sr_id).FirstOrDefault();
                if(tblsrid != 0)
                {
                    return "success";
                }
                else
                {
                    return "failure";
                }
            }
            return "failure";
        }

        internal byte[] getEncryptedMsg(int datakeyid)
        {
            byte[] datafile = db.tbl_datakey.Where(x => x.tbldatakey_id == datakeyid).Select(x => x.datafile).FirstOrDefault();
            return datafile;
        }

        internal string getDataKey(int datakeyid)
        {
            string datakey = db.tbl_datakey.Where(x => x.tbldatakey_id == datakeyid).Select(x => x.datakey).FirstOrDefault();
            return datakey;
        }
    }
}