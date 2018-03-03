using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading;
using TechnicalSupportService.Enums;
using TechnicalSupportService.Models;
using TechnicalSupportService.Repository.Context;
using TechnicalSupportService.Repository.DTO;

namespace TechnicalSupportService.Entities
{
    public class Requests
    {
        private static volatile Requests _instance;
        private static readonly object SyncRoot = new object();
        private readonly ConcurrentDictionary<string, RequestModel> _requestDict = new ConcurrentDictionary<string, RequestModel>();
        private static int _countRequest = 0;
        public int CountRequest => _countRequest;

        private readonly int _beginSpanSec;
        private readonly int _endSpanSec;
        readonly Random _random = new Random();

        private Requests()
        {
            _beginSpanSec = int.Parse(ConfigurationManager.AppSettings["BeginSpanSec"]);
            _endSpanSec = int.Parse(ConfigurationManager.AppSettings["EndSpanSec"]);

            using (var context = new HistoryContext())
            {
                foreach (var history in context.Histories.Where(w=> w.RequestStatus == RequestStatusType.NotProcessed))
                {
                    var request = new RequestModel
                    {
                        ID = history.RequestID,
                        Status = history.RequestStatus
                    };
                    this.Add(request, history.RequestStoreTime);
                }
            }
        }

        public static Requests Instance
        {
            get
            {
                if (_instance != null) return _instance;

                lock (SyncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new Requests();
                    }
                }
                return _instance;
            }
        }

        public bool Add(RequestModel requestModel, DateTime? requestStoreTime)
        {
            if (_requestDict.ContainsKey(requestModel.ID))
                return true;

            if (!_requestDict.TryAdd(requestModel.ID, requestModel))
                return false;

            Interlocked.Increment(ref _countRequest);

            using (var context = new HistoryContext())
            {
                context.Histories.Add(new HistoryDTO
                {
                    RequestID = requestModel.ID,
                    RequestStoreTime = requestStoreTime
                });
                context.SaveChangesAsync();
            }

            return true;
        }

        public bool Remove(string requestID)
        {
            if (!_requestDict.ContainsKey(requestID)) return true;
            
            if (!_requestDict.TryGetValue(requestID, out var removeRequestModel))
                return false;

            if (removeRequestModel.Status != RequestStatusType.Involved)
                return false;

            if (!_requestDict.TryRemove(requestID, out removeRequestModel))
                return false;

            Interlocked.Decrement(ref _countRequest);

            using (var context = new HistoryContext())
            {
                var removeField = context.Histories.FirstOrDefault(w => w.RequestID == requestID);
                if (removeField != null)
                {
                    removeField.Deleted = true;
                    context.Histories.AddOrUpdate(removeField);
                    context.SaveChangesAsync();
                }
            }

            return true;
        }

        public bool ChangeStatus(string requestID, RequestStatusType status)
        {
            if (!_requestDict.ContainsKey(requestID)) return false;

            if (!_requestDict.TryGetValue(requestID, out var changeRequestModel))
                return false;

            if (changeRequestModel.Status == status)
                return true;

            RequestModel newRequestModel = new RequestModel
            {
                ID = changeRequestModel.ID,
                Status = status
            };

            var success = _requestDict.TryUpdate(requestID, newRequestModel, changeRequestModel);

            if (!success) return false;

            using (var context = new HistoryContext())
            {
                var changeField = context.Histories.FirstOrDefault(w => w.RequestID == requestID);
                if (changeField != null)
                {
                    changeField.RequestStatus = status;
                    context.Histories.AddOrUpdate(changeField);
                    context.SaveChangesAsync();
                }
            }

            return true;
        }

        public RequestModel GetRequestModel(string requestID)
        {
            _requestDict.TryGetValue(requestID, out var requestModel);
            return requestModel;
        }

        public RequestModel GetStoredRequestModel(int? checkMaxStore)
        {
            RequestModel storedRequest;
            if (checkMaxStore.HasValue)
            {
                var now = DateTime.Now;
                storedRequest = _requestDict
                    .Where(w => w.Value.Status == RequestStatusType.NotProcessed &&
                                now.Subtract(w.Value.StoreTime).Minutes >= checkMaxStore).Select(s => s.Value)
                    .FirstOrDefault();
            }
            else
            {
                storedRequest = _requestDict
                    .Where(w => w.Value.Status == RequestStatusType.NotProcessed ).Select(s => s.Value)
                    .LastOrDefault();
            }

            return storedRequest;
        }

        public void RunRequest(string employeeID, string requestID)
        {
            int waitSec = _random.Next(_beginSpanSec, _endSpanSec);
            Thread.Sleep(waitSec * 1000);

            using (var context = new HistoryContext())
            {
                var changeField = context.Histories.FirstOrDefault(w => w.RequestID == requestID);
                if (changeField != null)
                {
                    changeField.RequestStatus = RequestStatusType.Processed;
                    changeField.Deleted = true;
                    changeField.OperationTime = waitSec;
                    context.Histories.AddOrUpdate(changeField);
                    context.SaveChangesAsync();
                }
            }

            Employees.Instance.ChangeStatus(employeeID, EmployeeStatusType.Free);
            Requests.Instance.Remove(requestID);
        }
    }
}