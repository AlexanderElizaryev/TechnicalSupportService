using System.Configuration;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading;
using TechnicalSupportService.Entities;
using TechnicalSupportService.Enums;
using TechnicalSupportService.Repository.Context;

namespace TechnicalSupportService.Services
{
    public class QueueRequestsHandler: IQueueRequestsHandler
    {
        private int TdMin;
        private int TmMin;

        public QueueRequestsHandler()
        {
            TdMin = int.Parse(ConfigurationManager.AppSettings["TdMin"]);
            TmMin = int.Parse(ConfigurationManager.AppSettings["TmMin"]);
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
                DoOperationBody(TdMin, EmployeeType.Director);
                DoOperationBody(TmMin, EmployeeType.Manager);
                DoOperationBody(null, EmployeeType.Simple);
            }
            Interlocked.Decrement(ref _work);
        }

        private void DoOperationBody(int? maxStoreMin, EmployeeType employeeType)
        {
            var tdRequestModel = Requests.Instance.GetStoredRequestModel(maxStoreMin);
            if (tdRequestModel == null) return;
            if (Employees.Instance.CountFreeEmployees <= 0) return;

            var employee = Employees.Instance.GetFreeEmployee(employeeType);
            if (employee == null) return;

            if (!Employees.Instance.ChangeStatus(employee.ID, EmployeeStatusType.Work)) return;

            using (var context = new HistoryContext())
            {
                context.SetEmpployee(tdRequestModel.ID, employee.ID);
            }

            if (Requests.Instance.ChangeStatus(tdRequestModel.ID, RequestStatusType.Involved))
            {
                Requests.Instance.RunRequest(employee.ID, tdRequestModel.ID);
            }
            else
            {
                Employees.Instance.ChangeStatus(employee.ID, EmployeeStatusType.Free);
            }
        }
    }
}