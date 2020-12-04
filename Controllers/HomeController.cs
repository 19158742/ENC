using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ENC.Controllers
{
    public class HomeController : Controller
    {
        ReadFile r = new ReadFile();
        ValidateData v = new ValidateData();
        DataOperations d = new DataOperations();
        DBOperations db = new DBOperations();
        public ActionResult Index()
        {
            return View();
        }

       
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            try
            {
                int senderId = 0;
                //string processFile = "";
                if (file.ContentLength > 0)
                {
                    string senderFirstName = "abc";
                    string senderLastName = "pqr";
                    string senderEmail = "aucd@gmail.com";
                    string senderSecretWord = "yyyy";
                    string validationRes = v.validateSenderInformation(senderFirstName,senderLastName,senderEmail,senderSecretWord);
                    if (validationRes.Equals("success"))
                    {
                        string senderAttribute = d.generateSenderAttribute(senderFirstName, senderLastName, senderEmail, senderSecretWord);
                        if (senderAttribute != null)
                        {
                            //senderId = db.saveSenderInformation(senderFirstName, senderLastName, senderEmail, senderSecretWord, senderAttribute);
                        }
                        else
                        {
                            //Error occured while generating attribute
                        }
                    }   
                    else
                    {
                        //validation failed
                    }
                    if(senderId > 0)
                    {
                       /* processFile = r.ProcessThisFile(senderId,file);
                        if(processFile.Equals("success"))
                        {
                            //File Encrypted and Uploaded Successfully
                        }*/
                    }
                    else
                    {
                        //Cannot Process This File
                    }
                    

                }
                ViewBag.Message = "File Uploaded Successfully!!";
                return View();
            }
            catch(Exception ex)
            {
                ViewBag.Message = "File upload failed!!";
                return View();
            }
        }
            public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}