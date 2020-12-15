using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace ENC
{
    public class DataOperations
    {
        readonly BlobManager b = new BlobManager("azurecontainer");
        readonly CycladeManager c = new CycladeManager();
        internal string GenerateSenderAttribute(string senderFirstName, string senderLastName, string senderEmail, string senderSecretWord)
        {
            string attr = senderFirstName.Substring(0, 2) + senderLastName.Substring(1, 2) + senderEmail.Substring(2, 5) + senderSecretWord.Substring(0, 1);
            return attr;
        }

        public List<SelectListItem> GetSenderList()
        {
            DBOperations db = new DBOperations();
            List<tbl_senderinfo> result = db.GetSenderList();
            return (List<SelectListItem>)result.ToList().Select(x => new SelectListItem()
            {
                Text = x.sender_fname,
                Value = x.sender_id.ToString()
            });
        }

        internal void SaveSRAllocation(tbl_SR_allocation tbl_SR_allocation)
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
            int tempn = rf.GenerateN(r);
            int n = rf.ValidateTempN(tempn, tbl_SR_allocation.tbldatakey_id);
            string senderattr = db.tbl_senderinfo.Where(x => x.sender_id == tbl_SR_allocation.sender_id).Select(x => x.sender_attribute).FirstOrDefault().ToString();
            string receiverKey = rf.GenerateReceiverKey(n,senderattr);
            tbl_SR_allocation.receiver_key = receiverKey;
            dbo.SaveReceiverKeyOnServer(tbl_SR_allocation);
            SendEmailToReceiver(receiverKey, r.receiverEmail, tbl_SR_allocation);
        }

        private void SendEmailToReceiver(string receiverKey, string receiverEmail, tbl_SR_allocation tbl_SR_allocation)
        {
            DBModel db = new DBModel();
            string senderfname = db.tbl_senderinfo.Where(x => x.sender_id == tbl_SR_allocation.sender_id).Select(x => x.sender_fname).FirstOrDefault();
            string senderlname = db.tbl_senderinfo.Where(x => x.sender_id == tbl_SR_allocation.sender_id).Select(x => x.sender_lname).FirstOrDefault();
            string senderemail = db.tbl_senderinfo.Where(x => x.sender_id == tbl_SR_allocation.sender_id).Select(x => x.sender_email).FirstOrDefault();
            string filename = db.tbl_datakey.Where(x => x.tbldatakey_id == tbl_SR_allocation.tbldatakey_id).Select(x => x.datafilename).FirstOrDefault();
            string semail = Convert.ToString(ConfigurationManager.AppSettings["semailaddr"]);
            string spass = Convert.ToString(ConfigurationManager.AppSettings["epassword"]);
            var sEmail = new MailAddress(semail, "Sender");
            var reEmail = new MailAddress(receiverEmail, "Receiver");
            var password = spass;
            var sub = "New file available for download";
            var body = "Hello, New file is ready to download. The first name of sender is- "+senderfname+", last name - " + senderlname + ", email - " + senderemail + ", File name - " + filename + " and receiver key - " + receiverKey + ". Please use this information to download the file. Thank you.";
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(sEmail.Address, password)
            };
            using (var mess = new MailMessage(sEmail.Address, "shraddha2977@gmail.com")
            {
                Subject = sub,
                Body = body
            })
            {
                smtp.Send(mess);
            }
        }

       
        public string GetData(int sr_id, int datakeyid, string receiver_key, string sender_fname, string sender_lname, string sender_email)
        {
            DBOperations db = new DBOperations();
            ReadFile r = new ReadFile();
            string myclouddata;
            string resValidation = db.ValidateUserData(sr_id, datakeyid, receiver_key, sender_fname, sender_lname, sender_email);
            if (resValidation.Equals("success"))
            {
                string datakey = db.GetDataKey(datakeyid);
                string servicetype = db.GetServiceType(datakeyid);
                byte[] getencryptedmsg = null;
                if (servicetype.Equals("A"))
                {
                  getencryptedmsg =  DownloadDataFromCloud(datakeyid);//aws
                }
                else if (servicetype.Equals("Z"))
                {
                  getencryptedmsg = b.downloadAzuereData(datakeyid); //azure
                }
                else if (servicetype.Equals("C"))
                {
                  getencryptedmsg = c.downloadCycladeData(datakeyid); //cyclade              
                }
                else
                {
                  getencryptedmsg = db.GetEncryptedMsg(datakeyid); //getting encrypted data from local sql when testing on localhost
                }
                myclouddata = r.GetDecryptedData(getencryptedmsg, datakey);
                if (myclouddata.Contains("failure"))
                { return "failure"; }
                else
                {
                    return myclouddata;
                }
            }
            else
            {
                return "failure";
            }
        }

        private byte[] DownloadDataFromCloud(int datakeyid)
        {
            DBOperations db = new DBOperations();
            string filename = db.GetFileNameById(datakeyid);
            string bucketName = ConfigurationManager.AppSettings["BucketName"];
            string keyName = filename.Split('.')[0] +"_"+ Convert.ToString(datakeyid) +"." + filename.Split('.')[1];
            RegionEndpoint bucketRegion = RegionEndpoint.USEast1;
            string accesskey = ConfigurationManager.AppSettings["AWSAccessKey"];
            string secretkey = ConfigurationManager.AppSettings["AWSSecretKey"];
            string sessionToken = ConfigurationManager.AppSettings["AWSSessionToken"];
            var s3Client = new AmazonS3Client(accesskey, secretkey, sessionToken, bucketRegion);
            Stream mstream = new MemoryStream();
            using (s3Client)
            {                
                GetObjectRequest request = new GetObjectRequest { BucketName = bucketName, Key = keyName };
                using (GetObjectResponse response = s3Client.GetObject(request))
                {
                    response.ResponseStream.CopyTo(mstream);
                }
                mstream.Position = 0;             
            }
            byte[] bytes = ReadToEnd(mstream);//convert stream to bytes
            return bytes;
        }

        internal void UploadFileOnCloud(tbl_datakey objtbl_datakey, byte[] encrypted, HttpPostedFileBase file)
        {
            string keyName = Path.GetFileName(file.FileName).Split('.')[0] + "_" + objtbl_datakey.tbldatakey_id + "." + Path.GetFileName(file.FileName).Split('.')[1];
            Stream stream = new MemoryStream(encrypted);
            string bucketName = ConfigurationManager.AppSettings["BucketName"];
            RegionEndpoint bucketRegion = RegionEndpoint.USEast1;
            string accesskey = ConfigurationManager.AppSettings["AWSAccessKey"];
            string secretkey = ConfigurationManager.AppSettings["AWSSecretKey"];
            string sessionToken = ConfigurationManager.AppSettings["AWSSessionToken"];
            var s3Client = new AmazonS3Client(accesskey, secretkey,sessionToken, bucketRegion);
            var fileTransferUtility = new TransferUtility(s3Client);
            try
            {
                if (file.ContentLength > 0)
                {
                    var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = stream,
                        BucketName = bucketName,
                        StorageClass = S3StorageClass.StandardInfrequentAccess,
                        PartSize = 6291456, // 6 MB.  
                        Key = keyName,
                        CannedACL = S3CannedACL.PublicRead,
                        ContentType = "text/plain;charset=utf-8"
                        
                    };
                    fileTransferUtility.Upload(fileTransferUtilityRequest);
                    fileTransferUtility.Dispose();
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                   Console.WriteLine("Check the provided AWS Credentials.");
                }
                else
                {
                    Console.WriteLine("Error occurred: " + amazonS3Exception.Message);
                }
            
        }

    }

        public byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }
                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }

        }

       
    }
}