using TicketManagementApi.Services;

namespace TicketManagementApi.BackgroundServices;

public class TicketCleanupService : BackgroundService
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromSeconds(30);

    private readonly ITicketService _ticketService;
    private readonly ILogger<TicketCleanupService> _logger;

    public TicketCleanupService(ITicketService ticketService, ILogger<TicketCleanupService> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(SweepInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                _ticketService.CleanupResolvedTickets();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run resolved-ticket cleanup sweep.");
            }
        }
    }
}
