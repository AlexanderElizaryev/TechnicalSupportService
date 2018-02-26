using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using TechnicalSupportService.Business;
using TechnicalSupportService.Entities;
using TechnicalSupportService.Enums;

namespace TechnicalSupportService.Services
{
    public class QueueRequestsHandler: IQueueRequestsHandler
    {
        private int TdMin;
        private int TmMin;

        private readonly IRequestOperations _requestOperations;

        public QueueRequestsHandler(IRequestOperations requestOperations)
        {
            TdMin = int.Parse(ConfigurationManager.AppSettings["TdMin"]);
            TmMin = int.Parse(ConfigurationManager.AppSettings["TmMin"]);

            _requestOperations = requestOperations;
        }

        private int _work = 0;
        public int Work => _work;

        public void Start()
        {
            Interlocked.Increment(ref _work);
            ThreadPool.QueueUserWorkItem(Operation);
        }

        private void Operation(object state)
        {
            while (Requests.Instance.CountRequest > 0)
            {
                Task.Run( async () => await DoOperationBody(TdMin, EmployeeType.Director));
                Task.Run(async () => await DoOperationBody(TmMin, EmployeeType.Manager));
                Task.Run(async () => await DoOperationBody(null, EmployeeType.Simple));
            }
            Interlocked.Decrement(ref _work);
        }

        private Task DoOperationBody(int? maxStoreMin, EmployeeType employeeType)
        {
            var tdRequestModel = Requests.Instance.GetMaxStoredRequestModel(maxStoreMin);
            if (tdRequestModel == null) return null;
            if (Employees.Instance.CountFreeEmployees <= 0) return null;

            var employee = Employees.Instance.GetFreeEmployee(employeeType);
            if (employee == null) return null;

            if (!Employees.Instance.ChangeStatus(employee.ID, EmployeeStatusType.Work)) return null;
            if (Requests.Instance.ChangeStatus(tdRequestModel.ID, RequestStatusType.Involved))
            {
                _requestOperations.RunRequest(employee.ID, tdRequestModel.ID);
            }
            else
            {
                Employees.Instance.ChangeStatus(employee.ID, EmployeeStatusType.Free);
            }
            return null;
        }
    }
}