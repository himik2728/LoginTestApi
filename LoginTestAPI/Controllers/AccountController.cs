using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using LoginTest2.Models;

namespace LoginTest2.Controllers
{
    //[EnableCors("*", "*", "*")]
    [Authorize]
    public class AccountController : ApiController
    {
        private dbLoginTestEntities db = new dbLoginTestEntities();

        public HttpResponseMessage Get(int id)
        {
            Users u = new Users();
            u = db.Users.Find(id);

            return Request.CreateResponse(HttpStatusCode.OK, u);
        }
    }
}
