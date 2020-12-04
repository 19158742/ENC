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
using System.Net.Http;
using System.Configuration;
using Newtonsoft.Json;

namespace ENC
{
    public class ReadFile
    {
        ReceiverInfo ri = new ReceiverInfo();
        DataOperations d = new DataOperations();
        DBOperations db = new DBOperations();
        BlobManager b = new BlobManager("azurecontainer");
        internal string ProcessThisFile(int senderId,HttpPostedFileBase file)
        {
            //upload data screen
            string m = generateM(senderId,file);
            string datakey = generateDataKey(m);
            if (datakey != "")
            {
                 EncryptData(datakey, file, senderId);
            }
            //return ReadDataFile(file);
            return null;
        }

        

        public string generateReceiverKey(int n, string senderattr)
        {
            string nkey = Convert.ToString(n);
            int len = senderattr.Length;
            int randomnumber = 0;
            int finalnumber = 0;
            if (nkey.Contains("-"))
            {
                randomnumber = Convert.ToInt32(nkey.Split('-')[1]);
            }
            else if (nkey.IsEmpty())
            {
                randomnumber = 1; 
            }
            else
            {
                randomnumber = n;
            }
            finalnumber = randomnumber + len;
            return Convert.ToString(randomnumber);
        }

        private string EncryptData(string datakey, HttpPostedFileBase file, int senderId)
        {
            return ReadDataFile(datakey, file,senderId);
        }

        private void SaveDataKeyOnServer(int senderId, string datakey)
        {
            db.SaveDataKeyOnServer(senderId, datakey);
        }

        private string generateDataKey(string m)
        {
            if(m.IsEmpty())
            {
                m = Convert.ToString(ConfigurationManager.AppSettings["DefaultMValue"]); 
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
                WriteTofile(encrypted, file,senderId,datakey);
            }
            return null;
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
            string ciphertext = PlainToCipher(plainText, Key);
            using (AesManaged aes = new AesManaged())
            {  
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(ciphertext);
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
                    db.saveFileDataToDB(objtbl_datakey); //sql

                    //d.UploadFileOnCloud(objtbl_datakey, file); //aws s3
                    //b.UploadFileOnAzure(objtbl_datakey, file); //azure
                    //CycladeManager c = new CycladeManager();
                    //c.UploadFileOnCyclade(objtbl_datakey, file);// cyclade
                    return "success";
                }
                return "failure";
               // return null;
            }
            catch(Exception ex)
            {
                return null;
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

            string retrievedata =  CipherToPlain(decrypted, resultDataKey);
            return retrievedata;
        }

        private static string PlainToCipher(string plainText, byte[] key)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in plainText)
            {
                char currentChar = (char)c;
                char modifiedChar = ModifyChar(currentChar, key);
                sb.Append(modifiedChar);
            }
            return sb.ToString();
        }

        private string CipherToPlain(string decrypted, byte[] resultDataKey)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in decrypted)
            {
                char currentChar = (char)c;
                char originalChar = OriginalChar(currentChar, resultDataKey);
                sb.Append(originalChar);
            }
            return sb.ToString();
        }
        private static char ModifyChar(char currentChar, byte[] key)
        {
            string str = System.Text.Encoding.UTF8.GetString(key);
            int n = Convert.ToInt32(ConfigurationManager.AppSettings["DataKeyNumber"]);
            char num = str[n];
            int extractedShift = Convert.ToInt32(num.ToString());
            char c = '@';
            string filename = "primaryjsondata.json";
            using (StreamReader r = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"LogicFolder\" + filename))
            {
                int pos = 0;
                string json = r.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                foreach (var item in array)
                {
                    if (item.value == currentChar)
                    {
                        pos = Convert.ToInt32(item.key) + extractedShift;
                        if (pos > array.Count)
                        {
                            pos = pos - array.Count - 1;
                        }
                    }
                }
                foreach (var val in array)
                {
                    if (val.key == pos)
                    {
                        c = (char)val.value;
                    }
                }
            }
            if(c == '@')
            {
                c = ' ';
            }
            return c;
        }

        private char OriginalChar(char currentChar, byte[] resultDataKey)
        {
            string str = System.Text.Encoding.UTF8.GetString(resultDataKey);
            int n = Convert.ToInt32(ConfigurationManager.AppSettings["DataKeyNumber"]);
            char num = str[n];
            int extractedShift = Convert.ToInt32(num.ToString());
            char c = '@';
            string filename = "primaryjsondata.json";
            using (StreamReader r = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"LogicFolder\" + filename))
            {
                int pos = 0;
                bool flag = false;
                int notfoundvalue = 99;
                string json = r.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                foreach (var item in array)
                {
                    if (item.value == currentChar)
                    {
                        pos = Convert.ToInt32(item.key) - extractedShift;
                        if(pos < 0)
                        {
                            pos = pos + array.Count;
                        }
                        flag = true;
                    }
                }
                if (flag == false && pos == 0)
                {
                    pos = notfoundvalue;
                }
                foreach (var val in array)
                {
                    if (val.key == pos)
                    {
                        c = (char)val.value;
                    }
                }
                if(c == '@' || pos == notfoundvalue)
                {
                    c = ' ';
                }
            }
            return c;
        }

        static string Decrypt(byte[] cipherText, byte[] Key, byte[] IV)
        {
            string plaintext = null;
            try
            {
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
            }
            catch (Exception ex) { 
            
            }
            return plaintext;
        }
    }
}