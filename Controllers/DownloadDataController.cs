using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Services;
using ENC;
using ENC.Models;

namespace ENC.Controllers
{
    public class DownloadDataController : Controller
    {
        private readonly DBModel db = new DBModel();
        readonly DataOperations d = new DataOperations();
       
        // GET: DownloadData
        public ActionResult Index()
        {
            try
            {
                int receiver_id = db.tbl_receiverinfo.Where(x => x.receiver_email.Equals(User.Identity.Name)).Select(x => x.receiver_id).FirstOrDefault();
                var datakeyids = db.tbl_SR_allocation.Where(x => x.receiver_id == receiver_id).Select(x => new { datakeyid = x.tbldatakey_id, srid = x.sr_id }).ToList();
                List<DownloadDataModel> datalists = new List<DownloadDataModel>();
                ListDownloadDataModel listddl = new ListDownloadDataModel();
                foreach (var key in datakeyids)
                {
                    DownloadDataModel ddl = new DownloadDataModel();
                    string datafilename = db.tbl_datakey.Where(x => x.tbldatakey_id == key.datakeyid).Select(x => x.datafilename).FirstOrDefault();
                    ddl.datafilename = datafilename;
                    ddl.datakey_id = key.datakeyid;
                    ddl.sr_id = key.srid;
                    datalists.Add(ddl);
                }
                listddl.listdownloadDataModel = datalists;
                return View(listddl);
            }
            catch(Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }       

        // GET: DownloadData/Details/5
        public ActionResult Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tbl_SR_allocation tbl_SR_allocation = db.tbl_SR_allocation.Find(id);
                if (tbl_SR_allocation == null)
                {
                    return HttpNotFound();
                }
                return View(tbl_SR_allocation);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // GET: DownloadData/Create
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

        // POST: DownloadData/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "sr_id,sender_id,receiver_id,tbldatakey_id,receiver_key")] tbl_SR_allocation tbl_SR_allocation)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.tbl_SR_allocation.Add(tbl_SR_allocation);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View(tbl_SR_allocation);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // GET: DownloadData/Edit/5
        public ActionResult Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tbl_SR_allocation tbl_SR_allocation = db.tbl_SR_allocation.Find(id);
                if (tbl_SR_allocation == null)
                {
                    return HttpNotFound();
                }
                return View(tbl_SR_allocation);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // POST: DownloadData/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "sr_id,sender_id,receiver_id,tbldatakey_id,receiver_key")] tbl_SR_allocation tbl_SR_allocation)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(tbl_SR_allocation).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(tbl_SR_allocation);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // GET: DownloadData/Delete/5
        public ActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tbl_SR_allocation tbl_SR_allocation = db.tbl_SR_allocation.Find(id);
                if (tbl_SR_allocation == null)
                {
                    return HttpNotFound();
                }
                return View(tbl_SR_allocation);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // POST: DownloadData/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try {
                tbl_SR_allocation tbl_SR_allocation = db.tbl_SR_allocation.Find(id);
                db.tbl_SR_allocation.Remove(tbl_SR_allocation);
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
