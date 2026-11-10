using Service.Services.Interfaces;

namespace Janz.Api.BackgroundServices
{
    // Müştəri Stripe səhifəsini bağlayıb qayıtmayıbsa: ödəniş gözləyən sifariş bir müddətdən sonra həll olunur
    // (ödənibsə sifariş yaranır, ödənməyibsə silinir və məhsullar yenidən stokda/səbətdə qalır)
    public class AbandonedOrderCleaner : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AbandonedOrderCleaner> _logger;

        public AbandonedOrderCleaner(IServiceScopeFactory scopeFactory, ILogger<AbandonedOrderCleaner> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Başlanğıcda API-nin tam qalxmasına imkan verilir
            try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); } catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var payments = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                    var count = await payments.SettleStaleAsync();
                    if (count > 0)
                    {
                        _logger.LogInformation("Settled {Count} abandoned card orders", count);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Abandoned order cleanup failed");
                }

                try { await Task.Delay(Interval, stoppingToken); } catch (OperationCanceledException) { return; }
            }
        }
    }
}
