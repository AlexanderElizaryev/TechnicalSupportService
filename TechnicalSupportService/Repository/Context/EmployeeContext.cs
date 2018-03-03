using System;
using System.Data.Entity;
using TechnicalSupportService.Enums;
using TechnicalSupportService.Repository.DTO;

namespace TechnicalSupportService.Repository.Context
{
    public class EmployeeContext : DbContext
    {
        static EmployeeContext()
        {
            Database.SetInitializer<EmployeeContext>(new EmployeeInitializer());
        }

        public EmployeeContext() : base("DbConnection")
        { }

        public DbSet<EmployeeDTO> Employees { get; set; }
    }

    public class EmployeeInitializer : DropCreateDatabaseIfModelChanges<EmployeeContext>
    {
        protected override void Seed(EmployeeContext context)
        {
            for (int i = 0; i < 10; i++)
            {
                EmployeeDTO employee = new EmployeeDTO
                {
                    ID = Guid.NewGuid().ToString(),
                    Status = EmployeeStatusType.Free,
                    Type = EmployeeType.Simple
                };
                context.Employees.Add(employee);
            }

            EmployeeDTO employeeManager = new EmployeeDTO
            {
                ID = Guid.NewGuid().ToString(),
                Status = EmployeeStatusType.Free,
                Type = EmployeeType.Manager
            };
            context.Employees.Add(employeeManager);

            EmployeeDTO employeeDirector = new EmployeeDTO
            {
                ID = Guid.NewGuid().ToString(),
                Status = EmployeeStatusType.Free,
                Type = EmployeeType.Director
            };
            context.Employees.Add(employeeDirector);

            context.SaveChanges();
        }
    }
}