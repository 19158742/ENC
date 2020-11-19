using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
                byte[] getencryptedmsg = db.getEncryptedMsg(datakeyid); //getting encrypted data from sql
                string mydata = r.getDecryptedData(getencryptedmsg, datakey);

                byte[] getencryptedmsgfromcloud = downloadDataFromCloud(datakeyid);
                string myclouddata = r.getDecryptedData(getencryptedmsg, datakey);
                return mydata;
            }
            else
            {
                return "failure";
            }
        }

        private byte[] downloadDataFromCloud(int datakeyid)
        {
            DBOperations db = new DBOperations();
            string filename = db.getFileNameById(datakeyid);
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

        internal void UploadFileOnCloud(tbl_datakey objtbl_datakey, HttpPostedFileBase file)
        {
            string keyName = Path.GetFileName(file.FileName).Split('.')[0] + "_" + objtbl_datakey.tbldatakey_id + "." + Path.GetFileName(file.FileName).Split('.')[1];
            Stream stream = new MemoryStream(objtbl_datakey.datafile);
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
                        CannedACL = S3CannedACL.PublicRead
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

        private byte[] ReadToEnd(Stream stream)
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

        //internal HttpResponseMessage GenerateDataFile(tbl_datakey objtbl_datakey)
        //{

        //    byte[] content = objtbl_datakey.datafile;
        //    HttpContext context = HttpContext.Current;
        //    context.Response.BinaryWrite(content);
        //    context.Response.ContentType = "text/plain";
        //    context.Response.AppendHeader("Content-Length", content.Length.ToString());
        //    context.Response.OutputStream.Write(content, 0, (int)content.Length);
        //    context.Response.End();

        //    string fileName = Guid.NewGuid() + ".txt";
        //    Stream stream = new MemoryStream(content);
        //    HttpResponseMessage httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        //    httpResponseMessage.Content = new StreamContent(stream);
        //    httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
        //    httpResponseMessage.Content.Headers.ContentDisposition.FileName = fileName;
        //    httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        //    return httpResponseMessage;

        //}


    }
}