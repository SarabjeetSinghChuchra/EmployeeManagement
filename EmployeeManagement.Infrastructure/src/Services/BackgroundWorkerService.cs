using EmployeeManagement.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class BackgroundWorkerService : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;

    public BackgroundWorkerService(IBackgroundTaskQueue taskQueue)
    {
        _taskQueue = taskQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _taskQueue.ExecuteAsync(stoppingToken);
    }
}
