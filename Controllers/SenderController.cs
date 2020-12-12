using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ENC;
using System.Net.Http;

namespace ENC.Controllers
{
    public class SenderController : Controller
    {
        private readonly DBModel db = new DBModel();
        private readonly ReadFile r = new ReadFile();
        readonly DataOperations d = new DataOperations();
        readonly DBOperations objdbOperations = new DBOperations();
        // GET: Sender
        public ActionResult Index()
        {
            try
            {
                return View(db.tbl_senderinfo.ToList());
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // GET: Sender/Details/5
        public ActionResult Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tbl_senderinfo tbl_senderinfo = db.tbl_senderinfo.Find(id);
                if (tbl_senderinfo == null)
                {
                    return HttpNotFound();
                }
                return View(tbl_senderinfo);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // GET: Sender/Create
        public ActionResult Create()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // POST: Sender/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "sender_id,sender_fname,sender_lname,sender_email,secret_word,sender_attribute")] tbl_senderinfo tbl_senderinfo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //generate sender attribute
                    tbl_senderinfo.sender_attribute = d.GenerateSenderAttribute(tbl_senderinfo.sender_fname, tbl_senderinfo.sender_lname, tbl_senderinfo.sender_email, tbl_senderinfo.secret_word);
                    string res = objdbOperations.SaveSenderInformation(tbl_senderinfo);
                    if (res.Equals("success"))
                    {
                        return RedirectToAction("Index");
                    }
                    else if (res.Contains("failure"))
                    {
                        ViewBag.Er = res;
                        return View("ErrorMsg");
                    }
                }
                return View(tbl_senderinfo);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // GET: Sender/Edit/5
        public ActionResult Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tbl_senderinfo tbl_senderinfo = db.tbl_senderinfo.Find(id);
                if (tbl_senderinfo == null)
                {
                    return HttpNotFound();
                }
                return View(tbl_senderinfo);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        public ActionResult UploadFile(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tbl_senderinfo tbl_senderinfo = db.tbl_senderinfo.Find(id);
                if (tbl_senderinfo == null)
                {
                    return HttpNotFound();
                }
                return View(tbl_senderinfo);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        [HttpPost, ActionName("UploadFile")]
        [ValidateAntiForgeryToken]
        public ActionResult UploadFile(int id, HttpPostedFileBase file)
        {
            try
            {
                int cloudType = Convert.ToInt32(Request["ddlCloudList"].ToString());
                Console.WriteLine(DateTime.Now.TimeOfDay);
                r.ProcessThisFile(id, file, cloudType);
                Console.WriteLine(DateTime.Now.TimeOfDay);
                return RedirectToAction("UploadFile");
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // POST: Sender/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "sender_id,sender_fname,sender_lname,sender_email,secret_word,sender_attribute")] tbl_senderinfo tbl_senderinfo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(tbl_senderinfo).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(tbl_senderinfo);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // GET: Sender/Delete/5
        public ActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tbl_senderinfo tbl_senderinfo = db.tbl_senderinfo.Find(id);
                if (tbl_senderinfo == null)
                {
                    return HttpNotFound();
                }
                return View(tbl_senderinfo);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // POST: Sender/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                tbl_senderinfo tbl_senderinfo = db.tbl_senderinfo.Find(id);
                db.tbl_senderinfo.Remove(tbl_senderinfo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
