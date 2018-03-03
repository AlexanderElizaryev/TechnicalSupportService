using System;
using System.Data.Entity.Migrations;
using System.Linq;
using TechnicalSupportService.Entities;
using TechnicalSupportService.Enums;
using TechnicalSupportService.Models;
using TechnicalSupportService.Repository.Context;
using TechnicalSupportService.Services;

namespace TechnicalSupportService.Business
{
    public class RequestOperations: IRequestOperations
    {
        private readonly IQueueRequestsHandler _queueRequestsHandler ;

        public RequestOperations(IQueueRequestsHandler queueRequestsHandler)
        {
            _queueRequestsHandler = queueRequestsHandler;
        }

        public bool AddRequest(string id)
        {
            RequestModel newRequestModel = new RequestModel
            {
                ID = id,
                StoreTime = DateTime.Now,
                Status = RequestStatusType.NotProcessed
            };

            bool successAdd = Requests.Instance.Add(newRequestModel, DateTime.Now);

            var freeEmployee = Employees.Instance.GetFreeEmployee(EmployeeType.Simple);
            if (freeEmployee == null)
            {
                if (_queueRequestsHandler.Work == 0)
                {
                    _queueRequestsHandler.Start();    
                }
                return successAdd;
            }

            using (var context = new HistoryContext())
            {
                context.SetEmpployee(id, freeEmployee.ID);
            }

            var status = this.GetStatusRequest(id);
            if (!status.HasValue || status.Value != RequestStatusType.NotProcessed)
                return successAdd;

            bool successChangeStatus = Requests.Instance.ChangeStatus(id, RequestStatusType.Involved);
            if (successChangeStatus)
            {
                Requests.Instance.RunRequest(freeEmployee.ID, newRequestModel.ID);
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
    }
}