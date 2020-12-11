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
        private DBModel db = new DBModel();
        private ReadFile r = new ReadFile();
        DataOperations d = new DataOperations();
        DBOperations objdbOperations = new DBOperations();
        // GET: Sender
        public ActionResult Index()
        {
            return View(db.tbl_senderinfo.ToList());
        }

        // GET: Sender/Details/5
        public ActionResult Details(int? id)
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

        // GET: Sender/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Sender/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "sender_id,sender_fname,sender_lname,sender_email,secret_word,sender_attribute")] tbl_senderinfo tbl_senderinfo)
        {
            if (ModelState.IsValid)
            {
                //generate sender attribute
                tbl_senderinfo.sender_attribute = d.generateSenderAttribute(tbl_senderinfo.sender_fname, tbl_senderinfo.sender_lname, tbl_senderinfo.sender_email, tbl_senderinfo.secret_word);
                string res = objdbOperations.SaveSenderInformation(tbl_senderinfo);
                if (res.Equals("success"))
                {
                    return RedirectToAction("Index");
                }
            }
            return View(tbl_senderinfo);
        }

        // GET: Sender/Edit/5
        public ActionResult Edit(int? id)
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

        public ActionResult UploadFile(int? id)
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

        [HttpPost, ActionName("UploadFile")]
        [ValidateAntiForgeryToken]
        public ActionResult UploadFile(int id, HttpPostedFileBase file)
        {
            int cloudType = Convert.ToInt32(Request["ddlCloudList"].ToString());
            Console.WriteLine(DateTime.Now.TimeOfDay);
            r.ProcessThisFile(id,file,cloudType);
            Console.WriteLine(DateTime.Now.TimeOfDay);
            return RedirectToAction("UploadFile", ViewData["DisplayTime"]);
        }

        // POST: Sender/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "sender_id,sender_fname,sender_lname,sender_email,secret_word,sender_attribute")] tbl_senderinfo tbl_senderinfo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_senderinfo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tbl_senderinfo);
        }

        // GET: Sender/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: Sender/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tbl_senderinfo tbl_senderinfo = db.tbl_senderinfo.Find(id);
            db.tbl_senderinfo.Remove(tbl_senderinfo);
            db.SaveChanges();
            return RedirectToAction("Index");
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
