using HRMS.Interfaces;

namespace HRMS.Services
{
    /// <summary>
    /// A background service that periodically triggers the anniversary leave carry-forward process.
    /// </summary>
    /// <remarks>
    /// This service runs in the background and uses a <see cref="PeriodicTimer"/> 
    /// to invoke <see cref="ILeaveService.ProcessAnniversaryCarryForward"/> at scheduled intervals.
    /// It utilizes an <see cref="IServiceScope"/> to resolve scoped services like <see cref="ILeaveService"/> 
    /// within the singleton background worker.
    /// </remarks>
    public class AnniversaryBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        /// <summary> Initializes a new instance of the <see cref="AnniversaryBackgroundService"/> class. </summary>
        /// <param name="serviceProvider">The service provider to resolve dependencies.</param>
        public AnniversaryBackgroundService(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        /// <summary>
        /// Executes the background task loop.
        /// </summary>
        /// <param name="stoppingToken">A token to signal that the service is stopping.</param>
        /// <returns>A task representing the background operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<ILeaveService>();
                    await service.ProcessAnniversaryCarryForward();
                }
            }
        }
    }
}
