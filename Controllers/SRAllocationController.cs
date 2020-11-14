using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ENC;

namespace ENC.Controllers
{
    public class SRAllocationController : Controller
    {
        private DBModel db = new DBModel();
        DataOperations d = new DataOperations();
        DBOperations dbOp = new DBOperations();
        // GET: SRAllocation
        public ActionResult Index()
        {            
            List<SRInfo> srinfo = dbOp.getSRInfo();
            return View(srinfo.ToList());
        }

        // GET: SRAllocation/Details/5
        public ActionResult Details(int? id)
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

        // GET: SRAllocation/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SRAllocation/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "sr_id,sender_id,receiver_id,tbldatakey_id,receiver_key")] tbl_SR_allocation tbl_SR_allocation)
        {
            if (ModelState.IsValid)
            {
                d.saveSRAllocation(tbl_SR_allocation);
                return RedirectToAction("Index");
            }

            return View(tbl_SR_allocation);
        }

        // GET: SRAllocation/Edit/5
        public ActionResult Edit(int? id)
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

        // POST: SRAllocation/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "sr_id,sender_id,receiver_id,tbldatakey_id,receiver_key")] tbl_SR_allocation tbl_SR_allocation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_SR_allocation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tbl_SR_allocation);
        }

        // GET: SRAllocation/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: SRAllocation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tbl_SR_allocation tbl_SR_allocation = db.tbl_SR_allocation.Find(id);
            db.tbl_SR_allocation.Remove(tbl_SR_allocation);
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

        [HttpGet]
        public JsonResult FetchSenderIds() // its a GET, not a POST
        {
            var senders = new Dictionary<string, string>();
            senders = dbOp.GetAllSenders();
            return Json(senders, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult FetchReceiverIds()
        {
            var receivers = new Dictionary<string, string>();
            receivers = dbOp.GetAllReceivers();
            return Json(receivers, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult FetchDataFileIds(string sender_id)
        {
            var datafileids = new Dictionary<string, string>();
            datafileids = dbOp.GetAllDataFileIds(sender_id);
            return Json(datafileids, JsonRequestBehavior.AllowGet);
        }
    }
}
