using System;
using System.EnterpriseServices.Internal;
using System.IO;
using System.Text;
using System.Web;
using System.Security.Cryptography;
using WebGrease;
using System.Linq;
using System.Numerics;
using System.Web.WebPages;
using System.Data.Entity;
using System.Web.UI.WebControls;

namespace ENC
{
    public class ReadFile
    {
        ReceiverInfo ri = new ReceiverInfo();
        DataOperations d = new DataOperations();
        DBOperations db = new DBOperations();
        internal string ProcessThisFile(int senderId,HttpPostedFileBase file)
        {
            //upload data screen
            string m = generateM(senderId,file);
            string datakey = generateDataKey(m);
            //SaveDataKeyOnServer(senderId,datakey);
            if (datakey != "")
            {
                EncryptData(datakey, file, senderId);
            }

            ///// Process after entering receivers data on 2nd screen
            //int tempn = generateN(ri);
            //int n = validateTempN(tempn, m);
            //string receiverKey = generateReceiverKey(n);
            //SaveReceiverKeyOnServer(receiverKey,senderId);

            ///
            //string line = "";
            //return ReadDataFile(file);
            return "s";
        }

        

        public string generateReceiverKey(int n)
        {
            string nkey = Convert.ToString(n);
            if (nkey.IsEmpty())
            {
                return ""; //use sender's attribute

            }
            return nkey;
        }

        private void EncryptData(string datakey, HttpPostedFileBase file, int senderId)
        {
            ReadDataFile(datakey, file,senderId);
        }

        private void SaveDataKeyOnServer(int senderId, string datakey)
        {
            db.SaveDataKeyOnServer(senderId, datakey);
        }

        private string generateDataKey(string m)
        {
            if(m.IsEmpty())
            {
                return ""; 
                
            }
            return m;
        }

        public int validateTempN(int tempn, int datakeyid)
        {
            int mlength = db.getMLength(datakeyid);
            int tempnlength = Convert.ToString(tempn).Length;
            if(tempnlength > mlength)
            {
                tempn = tempn - mlength;
            }
            return tempn;
            // if length of tempn>m = n= n-m = updatevalue of tempn 
            //else return tempn            
        }

        public int generateN(ReceiverInfo ri)
        {

            int cntCharacters = countReceiverInfoCharacters(ri);
            long multi = cntCharacters * Convert.ToInt64(DateTime.Now.Ticks);
            string str = Convert.ToString(multi).Substring(0, 5);
            int n = Convert.ToInt32(str);
            return n;
            //Multiply operation of cnt and timestamp and take first 5 digits, then  generate number n
        }

        private int countReceiverInfoCharacters(ReceiverInfo ri)
        {
            return ri.receiverEmail.Length + ri.receiverName.Length + ri.senderid.Length + ri.datakeyid.Length;
        }

        private string generateM(int senderId, HttpPostedFileBase file)
        {
            int contentSize = file.ContentLength;
            long finaltempdata = Convert.ToInt64(DateTime.Now.Ticks) * contentSize * senderId;
            string getSpecificChar = Convert.ToString(finaltempdata);
            if (getSpecificChar.Contains('-'))
            {
                getSpecificChar = Convert.ToString(finaltempdata).Split('-')[1];
            }
            if (getSpecificChar.Length < 32)
            {
                while (getSpecificChar.Length != 32)
                {
                    getSpecificChar = getSpecificChar + "1";
                }
            }
            if (getSpecificChar.Length > 32) {
                while (getSpecificChar.Length != 32)
                    getSpecificChar = getSpecificChar.Remove(getSpecificChar.Length - 1, 1);
            }
            
            return getSpecificChar;
            //Generate integer m based on sender's attribute , filesize, currentday
        }

        private string ReadDataFile(string datakey, HttpPostedFileBase file, int senderId)//Encryption
        {           
            BinaryReader b = new BinaryReader(file.InputStream);
            byte[] binData = b.ReadBytes(file.ContentLength);

            string resultRawData = System.Text.Encoding.UTF8.GetString(binData);
            byte[] stringBytes = Encoding.ASCII.GetBytes(datakey);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(stringBytes);
            byte[] resultDataKey = stringBytes;
            using (AesManaged aes = new AesManaged())
            {
                byte[] encrypted = Encrypt(resultRawData, resultDataKey, aes.IV);
                return WriteTofile(encrypted, file,senderId,datakey);
            }
            //System.Text.Encoding.UTF8.GetString(encrypted) --output



            //result.Replace("\r\n", " ");
            //string[] lines = result.Split(' ');
            //foreach (string line in lines)
            //{
            //    if (line != null)
            //    {
            //        //if (line.Contains(stringToSearch))
            //        //{
            //        //    line.Replace(stringToSearch, "newstring");
            //        //    sbText.AppendLine(line);
            //        //}
            //        //else
            //        //{
            //        //    sbText.AppendLine(line);
            //        //}
            //    }
            //}
            
        }

        static byte[] Encrypt(string plainText, byte[] Key, byte[] IV)
        {
            byte[] encrypted;  
            using (AesManaged aes = new AesManaged())
            {  
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            } 
            return encrypted;
        }
        private string WriteTofile(byte[] encrypted, HttpPostedFileBase file, int senderId, string datakey)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    tbl_datakey objtbl_datakey = new tbl_datakey();
                    objtbl_datakey.sender_id = senderId;
                    objtbl_datakey.datafilename = Convert.ToString(fileName);
                    objtbl_datakey.datafile = encrypted;
                    objtbl_datakey.datakey = datakey;
                    db.saveFileDataToDB(objtbl_datakey);
                    return "success";
                }
                return "failure";
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

        //-----//

        
        public string getDecryptedData(byte[] encrypted,string datakey)
        {
            string decrypted = string.Empty;
            byte[] stringBytes = Encoding.ASCII.GetBytes(datakey);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(stringBytes);
            byte[] resultDataKey = stringBytes;
            using (AesManaged aes = new AesManaged())
            {
                 decrypted = Decrypt(encrypted, resultDataKey, aes.IV);
            }
            return decrypted;
        }
        static string Decrypt(byte[] cipherText, byte[] Key, byte[] IV)
        {
            string plaintext = null;
            using (AesManaged aes = new AesManaged())
            {
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cs))
                            plaintext = reader.ReadToEnd();
                    }
                }
            }
            return plaintext;
        }
    }
}