using HardwareStore.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HardwareStore.Services.Tasks
{
    public class DatabaseCleanup
    {
        private readonly AppDbContext _context;
        private readonly TimeSpan _recordLifetime;
        private readonly ILogger<DatabaseCleanup> _logger;

        public DatabaseCleanup(AppDbContext context, TimeSpan recordLifetime, ILogger<DatabaseCleanup> logger)
        {
            _context = context;
            _recordLifetime = TimeSpan.FromDays(30);
            _logger = logger;
        }

        public async Task DeleteOldOrderRecords(CancellationToken cancellationToken = default)
        {
            var cuttOffDate = DateTime.UtcNow - _recordLifetime;

            var oudatedOrders = await _context.Orders
                .Where(o => o.OrderDate < cuttOffDate)
                .ToListAsync(cancellationToken);

            if (oudatedOrders.Any())
            {
                _context.Orders.RemoveRange(oudatedOrders);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Deleted {Count} outdated records older than {CutoffDate}.", oudatedOrders.Count, cuttOffDate);
            }
            else
            {
                _logger.LogInformation("No outdated records found at {Time}.", DateTime.UtcNow);
            }
        }

        public async Task DeleteOldActivityLogs(CancellationToken cancellationToken = default)
        {
            var cuttOffDate = DateTime.UtcNow - _recordLifetime;

            var oudatedOrders = await _context.ActivityLogs
                .Where(o => o.Timestamp < cuttOffDate)
                .ToListAsync(cancellationToken);

            if (oudatedOrders.Any())
            {
                _context.ActivityLogs.RemoveRange(oudatedOrders);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Deleted {Count} outdated records older than {CutoffDate}.", oudatedOrders.Count, cuttOffDate);
            }
            else
            {
                _logger.LogInformation("No outdated records found at {Time}.", DateTime.UtcNow);
            }
        }


    }
}
