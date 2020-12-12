using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
namespace ENC
{
    public class BlobManager
    {
        private CloudBlobContainer blobContainer;

        public BlobManager(string ContainerName)
        {
            if (string.IsNullOrEmpty(ContainerName))
            {
                throw new ArgumentNullException("azurecontainer", "Container Name can't be empty");
            }
            try
            {
                string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["AzureConn"].ConnectionString;
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);

                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                blobContainer = cloudBlobClient.GetContainerReference(ContainerName);
                if (blobContainer.CreateIfNotExists())
                {
                    blobContainer.SetPermissions(
                        new BlobContainerPermissions
                        {
                            PublicAccess = BlobContainerPublicAccessType.Blob
                        }
                    );
                }
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
        }

        public string UploadFileOnAzure(tbl_datakey objtbl_datakey, HttpPostedFileBase file)
        {
            string AbsoluteUri;
            if (file == null || file.ContentLength == 0)
                return null;
            try
            {
                string keyName = Path.GetFileName(file.FileName).Split('.')[0] + "_" + objtbl_datakey.tbldatakey_id + "." + Path.GetFileName(file.FileName).Split('.')[1];
                Stream stream = new MemoryStream(objtbl_datakey.datafile);
                string FileName = keyName;
                CloudBlockBlob blockBlob;
                blockBlob = blobContainer.GetBlockBlobReference(FileName);
                blockBlob.Properties.ContentType = "text/plain;charset=utf-8";
                blockBlob.UploadFromStream(stream);
                AbsoluteUri = blockBlob.Uri.AbsoluteUri;
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
            return AbsoluteUri;
        }
        public List<string> BlobList()
        {
            List<string> _blobList = new List<string>();
            foreach (IListBlobItem item in blobContainer.ListBlobs())
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob _blobpage = (CloudBlockBlob)item;
                    _blobList.Add(_blobpage.Uri.AbsoluteUri.ToString());
                }
            }
            return _blobList;
        }

        public byte[] downloadAzuereData(int datakeyid)
        {
            DataOperations dop= new DataOperations();
            DBOperations db = new DBOperations();
            string filename = db.GetFileNameById(datakeyid);
            string keyName = filename.Split('.')[0] + "_" + Convert.ToString(datakeyid) + "." + filename.Split('.')[1];
            BlobManager BlobManagerObj = new BlobManager("azurecontainer");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(System.Configuration.ConfigurationManager.ConnectionStrings["AzureConn"].ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("azurecontainer");

            // Retrieve reference to a blob named "trimmedString"
            CloudBlockBlob blockBlob2 = container.GetBlockBlobReference(keyName);

            //string text;
            using (var memoryStream = new MemoryStream())
            {
                blockBlob2.DownloadToStream(memoryStream);
                // text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                byte[] bytes = dop.ReadToEnd(memoryStream);//convert stream to bytes
                return bytes;
            }
            //return text;

            //Stream mstream = new MemoryStream();
           
            
        }
    }

}