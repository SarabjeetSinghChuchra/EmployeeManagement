using System.Collections.Concurrent;
using EmployeeManagement.Application.Interfaces;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems =
        new ConcurrentQueue<Func<CancellationToken, Task>>();
    private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

    public async Task EnqueueAsync(Func<CancellationToken, Task> task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        _workItems.Enqueue(task);
        _signal.Release();
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await _signal.WaitAsync(cancellationToken);

            if (_workItems.TryDequeue(out var workItem))
            {
                try
                {
                    await workItem(cancellationToken);
                }
                catch (Exception ex)
                {
                    // Handle exception as needed
                    Console.WriteLine($"Error while processing task: {ex.Message}");
                }
            }
        }
    }
}
