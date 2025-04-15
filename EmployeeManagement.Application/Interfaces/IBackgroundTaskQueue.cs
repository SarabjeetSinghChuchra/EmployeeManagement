using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagement.Application.Interfaces
{
    public interface IBackgroundTaskQueue
    {
        Task EnqueueAsync(Func<CancellationToken, Task> task);
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
