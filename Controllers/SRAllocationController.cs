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
    [Authorize]
    public class SRAllocationController : Controller
    {
        private readonly DBModel db = new DBModel();
        readonly DataOperations d = new DataOperations();
        readonly DBOperations dbOp = new DBOperations();
        // GET: SRAllocation
        public ActionResult Index()
        {
            try
            {
                List<SRInfo> srinfo = dbOp.GetSRInfo();
                return View(srinfo.ToList());
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // GET: SRAllocation/Details/5
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

        // GET: SRAllocation/Create
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

        // POST: SRAllocation/Create
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
                    d.SaveSRAllocation(tbl_SR_allocation);
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

        // GET: SRAllocation/Edit/5
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

        // POST: SRAllocation/Edit/5
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

        // GET: SRAllocation/Delete/5
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

        // POST: SRAllocation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
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

        [HttpGet]
        public JsonResult FetchSenderIds() // its a GET, not a POST
        {
            try
            {
                var senders = new Dictionary<string, string>();
                senders = dbOp.GetAllSenders();
                return Json(senders, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return Json(ex, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult FetchReceiverIds()
        {
            try
            {
                var receivers = new Dictionary<string, string>();
                receivers = dbOp.GetAllReceivers();
                return Json(receivers, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return Json(ex, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult FetchDataFileIds(string sender_id)
        {
            Dictionary<string, string> datafileids = dbOp.GetAllDataFileIds(sender_id);
            return Json(datafileids, JsonRequestBehavior.AllowGet);
        }
    }
}
