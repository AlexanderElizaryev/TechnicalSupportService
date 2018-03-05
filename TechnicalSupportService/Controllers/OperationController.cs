using System.Web.Http;
using TechnicalSupportService.Business;

namespace TechnicalSupportService.Controllers
{
    [RoutePrefix("api/operation")]
    public class OperationController : ApiController
    {
        private readonly IRequestOperations _requestOperations;

        public OperationController(IRequestOperations requestOperations)
        {
            _requestOperations = requestOperations;
        }

        [HttpGet]
        [Route("{id}/status")]
        public string GetStatusRequest(string id)
        {
            return _requestOperations.GetStatusRequest(id).ToString();
        }

        [HttpGet]
        [Route("count")]
        public int GetCountRequest()
        {
            return _requestOperations.GetCountRequest();
        }

        [Route("{id}")]
        [HttpPut]
        public bool AddRequest(string id)
        {
            return _requestOperations.AddRequest(id);
        }

        [Route("{id}")]
        [HttpDelete]
        public bool RemoveRequest(string id)
        {
            return _requestOperations.RemoveRequest(id);
        }
    }
}