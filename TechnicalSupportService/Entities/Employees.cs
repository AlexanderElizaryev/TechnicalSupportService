using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using TechnicalSupportService.Enums;
using TechnicalSupportService.Models;

namespace TechnicalSupportService.Entities
{
    public class Employees
    {
        private static volatile Employees _instance;
        private static readonly object SyncRoot = new object();
        private readonly ConcurrentDictionary<string, EmployeeModel> _emplDict = new ConcurrentDictionary<string, EmployeeModel>();
        private static int _countFreeEmployees = 0;
        public int CountFreeEmployees => _countFreeEmployees;

        private Employees()
        {
            //TODO read DB History values
            //set _emplDict

        }

        public static Employees Instance
        {
            get
            {
                if (_instance != null) return _instance;

                lock (SyncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new Employees();
                    }
                }
                return _instance;
            }
        }

        public bool Add(EmployeeModel employeeModel)
        {
            if (_emplDict.ContainsKey(employeeModel.ID))
                return true;

            if (!_emplDict.TryAdd(employeeModel.ID, employeeModel))
                return false;

            //TODO async write in DB


            Interlocked.Increment(ref _countFreeEmployees);
            return true;
        }

        public bool Remove(string employeeID)
        {
            if (!_emplDict.ContainsKey(employeeID)) return true;

            if (!_emplDict.TryGetValue(employeeID, out var removeEmployeeModel))
                return false;

            if (removeEmployeeModel.Status == EmployeeStatusType.Work)
                return false;

            if (!_emplDict.TryRemove(employeeID, out removeEmployeeModel)) return false;

            if (removeEmployeeModel.Status == EmployeeStatusType.Free)
                Interlocked.Decrement(ref _countFreeEmployees);

            //TODO async write in DB

            return true;
        }

        public bool ChangeStatus(string employeeID, EmployeeStatusType status)
        {
            if (!_emplDict.ContainsKey(employeeID)) return false;

            if (!_emplDict.TryGetValue(employeeID, out var changeEmployeeModel))
                return false;

            if (changeEmployeeModel.Status == status)
                return true;

            EmployeeModel newEmployeeModel = new EmployeeModel
            {
                ID = changeEmployeeModel.ID,
                Status = status
            };

            if (!_emplDict.TryUpdate(employeeID, changeEmployeeModel, newEmployeeModel))
                return false;
            
            if (changeEmployeeModel.Status == EmployeeStatusType.Work && status == EmployeeStatusType.Free)
            {
                Interlocked.Increment(ref _countFreeEmployees);
            }
            if (changeEmployeeModel.Status == EmployeeStatusType.Free && status == EmployeeStatusType.Work)
            {
                Interlocked.Decrement(ref _countFreeEmployees);
            }

            //TODO async write in DB

            return true;
        }

        public EmployeeModel GetEmployeeModel(string employeeID)
        {
            _emplDict.TryGetValue(employeeID, out var employeeModel);
            return employeeModel;
        }

        public EmployeeModel GetFreeEmployee(EmployeeType employeeType)
        {
            var employee = _emplDict
                .Where(w => w.Value.Type == employeeType && w.Value.Status == EmployeeStatusType.Free)
                .Select(s => s.Value).FirstOrDefault();

            if (employee == null)
                return null;
            
            return this.ChangeStatus(employee.ID, EmployeeStatusType.Work) ? employee : null;
        }
    }
}