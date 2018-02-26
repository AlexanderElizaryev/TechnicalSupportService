using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechnicalSupportService.Services
{
    public interface IQueueRequestsHandler
    {
        void Start();

        int Work { get; }
    }
}
