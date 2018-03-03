using System.Data.Entity;
using System.Linq;
using TechnicalSupportService.Repository.DTO;

namespace TechnicalSupportService.Repository.Context
{
    public class HistoryContext : DbContext
    {
        public HistoryContext()
            : base("DbConnection")
        {
        }

        public DbSet<HistoryDTO> Histories { get; set; }

        public void SetEmpployee(string requestID, string employeeID)
        {
            var field = this.Histories.FirstOrDefault(w => w.RequestID == requestID);
            if (field == null) return;

            field.EmployeeID = employeeID;
            this.SaveChangesAsync();
        }
    }
}