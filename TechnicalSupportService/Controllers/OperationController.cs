using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace TechnicalSupportService.Controllers
{
    public class OperationController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string GetStatusRequest(int id)
        {
            return $"value:{id}";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public string AddRequest(int id, [FromBody]string value)
        {
            return $"add request:{id}";
        }

        // DELETE api/<controller>/5
        public string RemoveRequest(int id)
        {
            return $"remove request:{id}";
        }
    }
}