using Microsoft.EntityFrameworkCore;
using TodoApp.Contexts;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace TodoApp.Services.Concretes
{
    public class ExpireBackgroundService : BackgroundService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly SmtpService _smtpService;
        private readonly ILogger<ExpireBackgroundService> _logger;
        private Timer _timer;

        public ExpireBackgroundService(IDbContextFactory<AppDbContext> contextFactory, SmtpService smtpService, ILogger<ExpireBackgroundService> logger)
        {
            _contextFactory = contextFactory;
            _smtpService = smtpService;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(CheckExpiredItems, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));
            return Task.CompletedTask;
        }

        private async void CheckExpiredItems(object? state)
        {
            try
            {
                using (var context = _contextFactory.CreateDbContext())
                {
                    var expiredItems = await context.TodoItems
                        .Where(item => item.ExpireDateTime <= DateTime.Now.AddDays(1))
                        .ToListAsync();

                    foreach (var item in expiredItems)
                    {
                        var user = await context.Users.FindAsync(item.UserId);
                        if (user != null)
                        {
                            var mailSent = await _smtpService.SendMail(user.Email, "Expired", "Your todo has expired");
                            if (!mailSent)
                            {
                                _logger.LogError($"Failed to send email to {user.Email} for expired todo item {item.Id}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking expired items");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            await base.StopAsync(stoppingToken);
        }

        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
}
