using System;
using System.Configuration;
using System.Threading;
using TechnicalSupportService.Entities;
using TechnicalSupportService.Enums;
using TechnicalSupportService.Models;
using TechnicalSupportService.Services;

namespace TechnicalSupportService.Business
{
    public class RequestOperations: IRequestOperations
    {
        private readonly int _beginSpanSec;
        private readonly int _endSpanSec;
        readonly Random _random = new Random();

        private readonly IQueueRequestsHandler _queueRequestsHandler ;

        public RequestOperations(IQueueRequestsHandler queueRequestsHandler)
        {
            _queueRequestsHandler = queueRequestsHandler;

            _beginSpanSec = int.Parse(ConfigurationManager.AppSettings["BeginSpanSec"]);
            _endSpanSec = int.Parse(ConfigurationManager.AppSettings["EndSpanSec"]);
        }

        public bool AddRequest(string id)
        {
            RequestModel newRequestModel = new RequestModel
            {
                ID = id,
                StoreTime = DateTime.Now,
                Status = RequestStatusType.NotProcessed
            };

            bool successAdd = Requests.Instance.Add(newRequestModel);
            //TODO: add HISTORY

            var freeEmployee = Employees.Instance.GetFreeEmployee(EmployeeType.Simple);
            if (freeEmployee == null)
            {
                if (_queueRequestsHandler.Work == 0)
                {
                    _queueRequestsHandler.Start();    
                }
                return successAdd;
            }

            var status = this.GetStatusRequest(id);
            if (!status.HasValue || status.Value != RequestStatusType.NotProcessed)
                return successAdd;

            bool successChangeStatus = Requests.Instance.ChangeStatus(id, RequestStatusType.Involved);
            if (successChangeStatus)
            {
                RunRequest(freeEmployee.ID, newRequestModel.ID);
            }
            else
            {
                Employees.Instance.ChangeStatus(freeEmployee.ID, EmployeeStatusType.Free);
            }

            return successAdd;
        }

        public bool RemoveRequest(string id)
        {
            return Requests.Instance.Remove(id);
        }

        public int GetCountRequest()
        {
            return Requests.Instance.CountRequest;
        }

        public RequestStatusType? GetStatusRequest(string id)
        {
            var requestModel = Requests.Instance.GetRequestModel(id);
            return requestModel?.Status;
        }

        public void RunRequest(string employeeID, string requestID)
        {
            int waitSec = _random.Next(_beginSpanSec, _endSpanSec);
            Thread.Sleep(waitSec * 1000);

            //TODO: write in History

            Employees.Instance.ChangeStatus(employeeID, EmployeeStatusType.Free);
            Requests.Instance.Remove(requestID);
        }
    }
}