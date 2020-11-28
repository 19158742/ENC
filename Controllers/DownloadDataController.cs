﻿using System;
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
        private DBModel db = new DBModel();
        DataOperations d = new DataOperations();
       
        // GET: DownloadData
        public ActionResult Index()
        {
            //int receiver_id = db.tbl_receiverinfo.Where(x => x.receiver_email.Equals(User.Identity));
            int receiver_id = 1;
           
            var datakeyids = db.tbl_SR_allocation.Where(x => x.receiver_id == receiver_id).Select(x=>new { datakeyid = x.tbldatakey_id, srid= x.sr_id }).ToList();
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

        [HttpPost]
        public JsonResult Index(string datakey_id,string srid, string r_key , string s_name, string s_lname,string s_email)
        {
            string receiver_key = "-5998";
            string sender_fname = "shraddha";
            string sender_lname = "mhatre";
            string sender_email = "shraddha@gmail.com";
            string msg = d.getData(Convert.ToInt32(srid), Convert.ToInt32(datakey_id), receiver_key, sender_fname, sender_lname, sender_email);
            if (msg != null)
            {
                byte[] msgtobytes = Encoding.ASCII.GetBytes(msg);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.BinaryWrite(msgtobytes);
                context.Response.ContentType = "text/plain";
                context.Response.AppendHeader("Content-Length", msgtobytes.Length.ToString());
                context.Response.OutputStream.Write(msgtobytes, 0, (int)msgtobytes.Length);
                context.Response.End();

                string fileName = Guid.NewGuid() + ".txt";
                Stream stream = new MemoryStream(msgtobytes);
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                httpResponseMessage.Content = new StreamContent(stream);
                httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                httpResponseMessage.Content.Headers.ContentDisposition.FileName = fileName;
                httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
                if (httpResponseMessage.Content == null)
                {
                    httpResponseMessage.Content = new StringContent("");
                }
                //return httpResponseMessage;
                /* //Write to file on specific destination folder
                var path = @"D:\ABC\demo.txt";
                using (StreamWriter sw = new StreamWriter(path)) 
                {
                    sw.Write(msg);
                }*/
                //System.IO.File.WriteAllText(@"D:\ABC\", msg);// Write Final Data To File
                //string _FileName = Path.GetFileName(file.FileName);
                //string _path = Path.Combine(Server.MapPath("~/UploadedFiles"), "demo");
                //file.SaveAs(_path);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
            //return View(datalists);
        }

        // GET: DownloadData/Details/5
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

        // GET: DownloadData/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DownloadData/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "sr_id,sender_id,receiver_id,tbldatakey_id,receiver_key")] tbl_SR_allocation tbl_SR_allocation)
        {
            if (ModelState.IsValid)
            {
                db.tbl_SR_allocation.Add(tbl_SR_allocation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tbl_SR_allocation);
        }

        // GET: DownloadData/Edit/5
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

        // POST: DownloadData/Edit/5
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

        // GET: DownloadData/Delete/5
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

        // POST: DownloadData/Delete/5
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

       
    }
}
