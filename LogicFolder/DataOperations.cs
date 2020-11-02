﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ENC
{
    public class DataOperations
    { 
        internal string generateSenderAttribute(string senderFirstName, string senderLastName, string senderEmail, string senderSecretWord)
        {
            string attr = senderFirstName.Substring(0, 2) + senderLastName.Substring(1, 2) + senderEmail.Substring(2, 5) + senderSecretWord.Substring(0, 1);
            return attr;
        }

        public List<SelectListItem> getSenderList()
        {
            DBOperations db = new DBOperations();
            List<tbl_senderinfo> result = db.getSenderList();
            return (List<SelectListItem>)result.ToList().Select(x => new SelectListItem()
            {
                Text = x.sender_fname,
                Value = x.sender_id.ToString()
            });
        }

        internal void saveSRAllocation(tbl_SR_allocation tbl_SR_allocation)
        {
            DBOperations dbo = new DBOperations();
            DBModel db = new DBModel();
            ReceiverInfo r = new ReceiverInfo();
            ReadFile rf = new ReadFile();
            r.datakeyid = Convert.ToString(tbl_SR_allocation.tbldatakey_id);
            r.senderid = Convert.ToString(tbl_SR_allocation.sender_id);
            tbl_receiverinfo receiverInfo = db.tbl_receiverinfo.Where(x => x.receiver_id == tbl_SR_allocation.receiver_id).FirstOrDefault();
            r.receiverEmail = receiverInfo.receiver_email;
            r.receiverName = receiverInfo.receiver_name;
            int tempn = rf.generateN(r);
            int n = rf.validateTempN(tempn, tbl_SR_allocation.tbldatakey_id);
            string receiverKey = rf.generateReceiverKey(n);
            tbl_SR_allocation.receiver_key = receiverKey;
            dbo.SaveReceiverKeyOnServer(tbl_SR_allocation);
        }
        public string getData(int sr_id, int datakeyid, string receiver_key, string sender_fname, string sender_lname, string sender_email)
        {
            DBOperations db = new DBOperations();
            ReadFile r = new ReadFile();
            string resValidation = db.validateUserData(sr_id, datakeyid, receiver_key, sender_fname, sender_lname, sender_email);
            if (resValidation.Equals("success"))
            {
                string datakey = db.getDataKey(datakeyid);
                byte[] getencryptedmsg = db.getEncryptedMsg(datakeyid);
                string mydata = r.getDecryptedData(getencryptedmsg, datakey);
                return mydata;
            }
            else
            {
                return "failure";
            }
        }
    }
}