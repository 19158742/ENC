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
    public class ReceiverController : Controller
    {
        private DBModel db = new DBModel();

        // GET: Receiver
        public ActionResult Index()
        {
            try
            {
                return View(db.tbl_receiverinfo.ToList());
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // GET: Receiver/Details/5
        public ActionResult Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tbl_receiverinfo tbl_receiverinfo = db.tbl_receiverinfo.Find(id);
                if (tbl_receiverinfo == null)
                {
                    return HttpNotFound();
                }
                return View(tbl_receiverinfo);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // GET: Receiver/Create
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

        // POST: Receiver/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "receiver_id,receiver_name,receiver_email")] tbl_receiverinfo tbl_receiverinfo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.tbl_receiverinfo.Add(tbl_receiverinfo);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View(tbl_receiverinfo);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }


        // GET: Receiver/Edit/5
        public ActionResult Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tbl_receiverinfo tbl_receiverinfo = db.tbl_receiverinfo.Find(id);
                if (tbl_receiverinfo == null)
                {
                    return HttpNotFound();
                }
                return View(tbl_receiverinfo);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // POST: Receiver/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "receiver_id,receiver_name,receiver_email")] tbl_receiverinfo tbl_receiverinfo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(tbl_receiverinfo).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(tbl_receiverinfo);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // GET: Receiver/Delete/5
        public ActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tbl_receiverinfo tbl_receiverinfo = db.tbl_receiverinfo.Find(id);
                if (tbl_receiverinfo == null)
                {
                    return HttpNotFound();
                }
                return View(tbl_receiverinfo);
            }
            catch (Exception ex)
            {
                ViewBag.Er = ex.Message.ToString();
                return View("ErrorMsg");
            }
        }

        // POST: Receiver/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                tbl_receiverinfo tbl_receiverinfo = db.tbl_receiverinfo.Find(id);
                db.tbl_receiverinfo.Remove(tbl_receiverinfo);
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
