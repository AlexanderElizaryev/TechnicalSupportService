using TechnicalSupportService.Enums;

namespace TechnicalSupportService.Models
{
    public class EmployeeModel
    {
        public string ID { get; set; }
        public EmployeeStatusType Status { get; set; }
        public EmployeeType Type { get; set; }
    }
}