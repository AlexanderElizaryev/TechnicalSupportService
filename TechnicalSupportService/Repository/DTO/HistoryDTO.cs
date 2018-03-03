using System;
using TechnicalSupportService.Enums;

namespace TechnicalSupportService.Repository.DTO
{
    public class HistoryDTO
    {
        public HistoryDTO()
        {
            ID = Guid.NewGuid().ToString();
            Date = DateTime.Now;
            Deleted = false;
            RequestStatus = RequestStatusType.NotProcessed;
        }

        public string ID { get; set; }
        public DateTime Date { get; set; }
        public string EmployeeID { get; set; }
        public string RequestID { get; set; }
        public DateTime? RequestStoreTime { get; set; }
        public int OperationTime { get; set; }
        public RequestStatusType RequestStatus { get; set; }
        public bool Deleted { get; set; }
    }
}