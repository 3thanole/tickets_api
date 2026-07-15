using TicketManagementApi.Services;

namespace TicketManagementApi.BackgroundServices;

public class TicketCleanupService(ITicketService ticketService, ILogger<TicketCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(SweepInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                ticketService.CleanupResolvedTickets();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to run resolved-ticket cleanup sweep.");
            }
        }
    }
}
