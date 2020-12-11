using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace ENC.Controllers
{
    public class GetfileController : ApiController
    {
        // GET api/<controller>
        public HttpResponseMessage Get([FromUri] string datakey_id, string srid, string r_key, string s_name, string s_lname, string s_email)
        {
            DataOperations d = new DataOperations();
            string msg = d.getData(Convert.ToInt32(srid), Convert.ToInt32(datakey_id), r_key, s_name, s_lname, s_email);
            if (msg != null)
            {
                byte[] msgtobytes = Encoding.ASCII.GetBytes(msg);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.BinaryWrite(msgtobytes);
                context.Response.ContentType = "application/download";
                context.Response.AppendHeader("Content-Length", msgtobytes.Length.ToString());
                context.Response.OutputStream.Write(msgtobytes, 0, (int)msgtobytes.Length);
                context.Response.End();

                string fileName = Guid.NewGuid() + ".txt";
                Stream stream = new MemoryStream(msgtobytes);
                HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                httpResponseMessage.Content = new StreamContent(stream);
                httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                httpResponseMessage.Content.Headers.ContentDisposition.FileName = fileName;
                httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/download");
                if (httpResponseMessage.Content == null)
                {
                    httpResponseMessage.Content = new StringContent("hello no content returned");
                }
                return httpResponseMessage;
            }
            return null;
        }

        

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}