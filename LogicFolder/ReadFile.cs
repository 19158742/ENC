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
        readonly ReceiverInfo ri = new ReceiverInfo();
        readonly DataOperations d = new DataOperations();
        readonly DBOperations db = new DBOperations();
        readonly BlobManager b = new BlobManager("azurecontainer");
        internal string ProcessThisFile(int senderId,HttpPostedFileBase file, int cloudType)
        {
            //upload data screen
            string m = GenerateM(senderId,file);
            string datakey = GenerateDataKey(m);
            if (datakey != "")
            {
                 EncryptData(datakey, file, senderId,cloudType);
            }
            return null;
        }

        

        public string GenerateReceiverKey(int n, string senderattr)
        {
            string nkey = Convert.ToString(n);
            int len = senderattr.Length;
            int randomnumber;
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
            int finalnumber = randomnumber + len;
            return Convert.ToString(finalnumber);
        }

        private string EncryptData(string datakey, HttpPostedFileBase file, int senderId, int cloudType)
        {
            return ReadDataFile(datakey, file,senderId,cloudType);
        }

        private string GenerateDataKey(string m)
        {
            if(m.IsEmpty())
            {
                m = Convert.ToString(ConfigurationManager.AppSettings["DefaultMValue"]); 
            }
            return m;
        }

        public int ValidateTempN(int tempn, int datakeyid)
        {
            int mlength = db.GetMLength(datakeyid);
            int tempnlength = Convert.ToString(tempn).Length;
            if(tempnlength > mlength)
            {
                tempn -= mlength;
            }
            return tempn;
            // if length of tempn>m = n= n-m = updatevalue of tempn 
            //else return tempn            
        }

        public int GenerateN(ReceiverInfo ri)
        {

            int cntCharacters = CountReceiverInfoCharacters(ri);
            long multi = cntCharacters * Convert.ToInt64(DateTime.Now.Ticks);
            string str = Convert.ToString(multi).Substring(0, 5);
            int n = Convert.ToInt32(str);
            return n;
            //Multiply operation of count and timestamp and take first 5 digits, then  generate number n
        }

        private int CountReceiverInfoCharacters(ReceiverInfo ri)
        {
            return ri.receiverEmail.Length + ri.receiverName.Length + ri.senderid.Length + ri.datakeyid.Length;
        }

        private string GenerateM(int senderId, HttpPostedFileBase file)
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
                    getSpecificChar += "1";
                }
            }
            if (getSpecificChar.Length > 32) {
                while (getSpecificChar.Length != 32)
                    getSpecificChar = getSpecificChar.Remove(getSpecificChar.Length - 1, 1);
            }
            
            return getSpecificChar;
        }

        private string ReadDataFile(string datakey, HttpPostedFileBase file, int senderId, int cloudType)//Encryption
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
                WriteTofile(encrypted, file,senderId,datakey, cloudType);
            }
            return null;

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

    

        private string WriteTofile(byte[] encrypted, HttpPostedFileBase file, int senderId, string datakey, int cloudType)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    tbl_datakey objtbl_datakey = new tbl_datakey
                    {
                        sender_id = senderId,
                        datafilename = Convert.ToString(fileName),                        
                        datakey = datakey
                    };
                    if (cloudType == 1)
                    {
                        objtbl_datakey.datafile = Encoding.Default.GetBytes("A");
                    }
                    else if (cloudType == 2)
                    {
                        objtbl_datakey.datafile = Encoding.Default.GetBytes("Z");
                    }
                    else if (cloudType == 3)
                    {
                        objtbl_datakey.datafile = Encoding.Default.GetBytes("C");
                    }
                    else
                    {
                        objtbl_datakey.datafile = encrypted;// if you want to save encrypted data in database
                    }
                    db.SaveFileDataToDB(objtbl_datakey); //rds sql
                    if (cloudType == 1)
                    {
                        d.UploadFileOnCloud(objtbl_datakey,encrypted, file); //aws s3
                    }
                    else if (cloudType == 2)
                    {
                        b.UploadFileOnAzure(objtbl_datakey,encrypted, file); //azure
                    }
                    else if (cloudType == 3)
                    {
                        CycladeManager c = new CycladeManager();
                        c.UploadFileOnCyclade(objtbl_datakey,encrypted, file);// cyclade
                    }
                    return "success";
                }
                return "failure";
            }
            catch(Exception ex)
            {
                return "failure" + ex.Message.ToString();
            }
        }



        
        public string GetDecryptedData(byte[] encrypted,string datakey)
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
            if(decrypted.Contains("failureLength of the data to decrypt is invalid."))
            {
                return "failure";
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
                            pos += array.Count;
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
                return "failure" + ex.Message.ToString();
            }
            return plaintext;
        }
    }
}