using System;
using TechnicalSupportService.Enums;

namespace TechnicalSupportService.Models
{
    public class RequestModel
    {
        public string ID { get; set; }
        public DateTime StoreTime { get; set; }
        public RequestStatusType Status { get; set; }
    }
}