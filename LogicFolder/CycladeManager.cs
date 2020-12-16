using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using Renci.SshNet;

namespace ENC
{ 
    public class CycladeManager
    {
        string user = ConfigurationManager.AppSettings["CycladeUserName"];
        string pass = ConfigurationManager.AppSettings["CycladeHost"];
        string host = ConfigurationManager.AppSettings["CycladePassword"];
        public CycladeManager()
        {
            //Set up the SSH connection
            using (var client = new SshClient(host, user, pass))
            {
                //Start the connection
                client.Connect();
                var output = client.RunCommand("echo connected");     
                client.Disconnect();
                Console.WriteLine(output.Result);
            }
        }

        public string UploadFileOnCyclade(tbl_datakey objtbl_datakey, byte[] encrypted, HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return null;
            try
            {
                string keyName = Path.GetFileName(file.FileName).Split('.')[0] + "_" + objtbl_datakey.tbldatakey_id + "." + Path.GetFileName(file.FileName).Split('.')[1];
                Stream stream = new MemoryStream(encrypted);
                string FileName = keyName;
                using (SftpClient sftp = new SftpClient(host, 22, user, pass))
                {
                    sftp.Connect();
                    if (sftp.IsConnected)
                    {
                        sftp.UploadFile(stream, sftp.WorkingDirectory + "/" + FileName,true,null);
                        sftp.Disconnect();  
                    }
                }
                return "success";
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
        }

            public byte[] downloadCycladeData(int datakeyid)
            {
            DataOperations dop = new DataOperations();
            DBOperations db = new DBOperations();
            string filename = db.GetFileNameById(datakeyid);
            string keyName = filename.Split('.')[0] + "_" + Convert.ToString(datakeyid) + "." + filename.Split('.')[1];
            
                using (SftpClient sftp = new SftpClient(host, 22, user, pass))
                {
                    sftp.Connect();
                    if (sftp.IsConnected)
                    {
                        string path = sftp.WorkingDirectory + "/" + keyName;
                    Stream mstream = new MemoryStream();
                    sftp.DownloadFile(path, mstream);
                    mstream.Position = 0;
                    byte[] bytes = dop.ReadToEnd(mstream);//convert stream to bytes
                    return bytes;
                    }
                }
            return null;
            }
    }
}