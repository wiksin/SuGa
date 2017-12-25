using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Nop.Plugin.Misc.WebApiServices.Controllers.Version1
{
    //[ApiVersion1RoutePrefix("default")]
    public class DefaultController : ApiController   
    {
        // GET: api/Default
        public IEnumerable<string> Get()
        {
            return new string[] { "value1 from v1", "value2" };
        }
        //[Route("{id}")]
        // GET: api/Default/5
        public string Get(int id)
        {
            return "value";
        }
        [Route]
        // POST: api/Default
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Default/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Default/5
        public void Delete(int id)
        {
        }
    }
}
