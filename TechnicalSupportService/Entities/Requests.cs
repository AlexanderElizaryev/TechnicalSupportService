using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Linq;
using System.Threading;
using TechnicalSupportService.Enums;
using TechnicalSupportService.Models;

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
            //TODO read DB History values
            _beginSpanSec = int.Parse(ConfigurationManager.AppSettings["BeginSpanSec"]);
            _endSpanSec = int.Parse(ConfigurationManager.AppSettings["EndSpanSec"]);

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

        public bool Add(RequestModel requestModel)
        {
            if (_requestDict.ContainsKey(requestModel.ID))
                return true;

            if (!_requestDict.TryAdd(requestModel.ID, requestModel))
                return false;
            
            //TODO async write in DB


            Interlocked.Increment(ref _countRequest);
            return true;
        }

        public bool Remove(string requestID)
        {
            if (!_requestDict.ContainsKey(requestID)) return true;
            
            if (!_requestDict.TryGetValue(requestID, out var removeRequestModel))
                return false;

            if (removeRequestModel.Status == RequestStatusType.Involved)
                return false;

            if (!_requestDict.TryRemove(requestID, out removeRequestModel))
                return false;

            //TODO async write in DB


            Interlocked.Decrement(ref _countRequest);
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

            return _requestDict.TryUpdate(requestID, changeRequestModel, newRequestModel);
            //TODO async write in DB
        }

        public RequestModel GetRequestModel(string requestID)
        {
            _requestDict.TryGetValue(requestID, out var requestModel);
            return requestModel;
        }

        public RequestModel GetMaxStoredRequestModel(int? checkMaxStore)
        {
            var maxStoreTime = _requestDict.Where(w => w.Value.Status == RequestStatusType.NotProcessed)
                .Max(m => m.Value.StoreTime);

            var minSubstract = DateTime.Now.Subtract(maxStoreTime).Minutes;
            if (checkMaxStore.HasValue && minSubstract < checkMaxStore)
                return null;

            return _requestDict
                .Where(w => w.Value.Status == RequestStatusType.NotProcessed && w.Value.StoreTime == maxStoreTime)
                .Select(s => s.Value).FirstOrDefault();
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