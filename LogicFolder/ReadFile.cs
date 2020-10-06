using System;
using System.EnterpriseServices.Internal;
using System.IO;
using System.Text;
using System.Web;
using WebGrease;

namespace ENC
{
    public class ReadFile
    {
        ReceiverInfo ri = new ReceiverInfo();
        DataOperations d = new DataOperations();
        internal string ProcessThisFile(int senderId,HttpPostedFileBase file)
        {

            int m = generateM(senderId,file);
            string datakey = generateDataKey(m);
            SaveDataKeyOnServer(datakey);
            EncryptData(datakey);
            ///// Process after entering receivers data on 2nd screen
            int tempn = generateN(ri);
            int n = validateTempN(tempn, m);
            string receiverKey = generateReceiverKey(n);
            SaveReceiverKeyOnServer(receiverKey,senderId);

            ///
            //string line = "";
            return ReadDataFile(file);
        }

        private void SaveReceiverKeyOnServer(string receiverKey, int senderId)
        {
            throw new NotImplementedException();
        }

        private string generateReceiverKey(int n)
        {
            throw new NotImplementedException();
        }

        private void EncryptData(string datakey)
        {
            throw new NotImplementedException();
        }

        private void SaveDataKeyOnServer(string datakey)
        {
            throw new NotImplementedException();
        }

        private string generateDataKey(int m)
        {
            throw new NotImplementedException();
        }

        private int validateTempN(int tempn, int m)
        {
            // if tempn>m = n= n-m = updatevalue of tempn and again check if tempn>m
            //else return tempn
            throw new NotImplementedException();
        }

        private int generateN(ReceiverInfo ri)
        {

            int cnt = countReceiverInfoCharacters(ri);
            //Mod operation of cnt and timestamp then  generate number n
            throw new NotImplementedException();
        }

        private int countReceiverInfoCharacters(ReceiverInfo ri)
        {
            throw new NotImplementedException();
        }

        private int generateM(int senderId, HttpPostedFileBase file)
        {
            //Generate integer m based on sender's attribute , filesize, timestamp
            throw new NotImplementedException();
        }

        private string ReadDataFile(HttpPostedFileBase file)
        {
            string stringToSearch = "abc";
            StringBuilder sbText = new StringBuilder();
            BinaryReader b = new BinaryReader(file.InputStream);
            byte[] binData = b.ReadBytes(file.ContentLength);

            string result = System.Text.Encoding.UTF8.GetString(binData);
            result.Replace("\r\n", " ");
            string[] lines = result.Split(' ');
            foreach (string line in lines)
            {
                if (line != null)
                {
                    if (line.Contains(stringToSearch))
                    {
                        line.Replace(stringToSearch, "newstring");
                        sbText.AppendLine(line);
                    }
                    else
                    {
                        sbText.AppendLine(line);
                    }
                }
            }
            return WriteTofile(sbText);
        }

        private string WriteTofile(StringBuilder sbText)
        {
            try
            {
                return "success";
            }
            catch(Exception ex)
            {
                return "failure";
            }
            //System.IO.File.WriteAllText("D:\\UploadedFiles", sbText.ToString());// Write Final Data To File
            //string _FileName = Path.GetFileName(file.FileName);
            //string _path = Path.Combine(Server.MapPath("~/UploadedFiles"), _FileName);
            //file.SaveAs(_path);
        }
    }
}