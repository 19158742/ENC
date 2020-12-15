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
        public static int failurecount = 0;
        // GET api/<controller>
        public HttpResponseMessage Get([FromUri] string datakey_id, string srid, string r_key, string s_name, string s_lname, string s_email)
        {
            HttpResponseMessage httpResponseMessage;
            try
            {
                DataOperations d = new DataOperations();
                string msg = d.GetData(Convert.ToInt32(srid), Convert.ToInt32(datakey_id), r_key, s_name, s_lname, s_email);
                if (msg != null && msg != "failure")
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
                    httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                    httpResponseMessage.Content = new StreamContent(stream);
                    httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                    {
                        FileName = fileName
                    };
                    httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/download");
                    if (httpResponseMessage.Content == null)
                    {
                        httpResponseMessage.Content = new StringContent("Hello no content returned");
                    }
                    return httpResponseMessage;
                }
                if(msg == "failure")
                {
                    failurecount += 1;
                    if(failurecount >= 3)
                    {
                        httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                        httpResponseMessage.Content = new StringContent("Content not found");
                        return httpResponseMessage;
                    }
                }
                return null;
            }
            catch (Exception ex) {
                httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                httpResponseMessage.Content = new StringContent(ex.Message.ToString());
                return httpResponseMessage;
            }
        }

        

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value" + id.ToString();
        }

       

      
    }
}