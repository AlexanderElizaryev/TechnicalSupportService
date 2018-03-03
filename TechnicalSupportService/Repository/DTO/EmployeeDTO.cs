using TechnicalSupportService.Enums;

namespace TechnicalSupportService.Repository.DTO
{
    public class EmployeeDTO
    {
        public string ID { get; set; }
        public EmployeeStatusType Status { get; set; }
        public EmployeeType Type { get; set; }
    }
}