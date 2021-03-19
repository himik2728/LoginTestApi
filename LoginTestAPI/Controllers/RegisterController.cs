using LoginTest2.Logic;
using LoginTest2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LoginTest2.Controllers
{
    public class RegisterController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Post(Users model)
        {
            string errmsg = "";
            DbRepository db = new DbRepository();
            if (model != null)
            {
                model = db.RegisterUser(model, out errmsg);
            }
            else
            {
                errmsg = "Empty Model!";
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { errmsg, model });
        }
    }
}
