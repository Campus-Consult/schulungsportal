using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Schulungsportal_2.Data;
using Schulungsportal_2.Models.Schulungen;
using Schulungsportal_2.Models;

namespace Schulungsportal_2.Services
{
    public class AnwesenheitslisteReminderService : IHostedService, IDisposable
    {

        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Timer _timer;
        private readonly IServiceScopeFactory scopeFactory;

        public AnwesenheitslisteReminderService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            logger.Info("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, 
                TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        // wird jede Stunde aufgerufen um zu überprüfen, ob noch Schulungen existieren,
        // deren Organisator nicht benachrichtigt wurde
        private void DoWork(object state)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                ApplicationDbContext _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                MailingHelper mailingHelper = scope.ServiceProvider.GetRequiredService<MailingHelper>();
                SchulungRepository schulungRepository = new SchulungRepository(_context);
                var ungeprüfteSchulungen = schulungRepository.GetUngeprüfteSchulungen()
                    .Where(s => !s.GeprüftReminderSent);
                logger.Info("Schulungen zum Überprüfen: "+ungeprüfteSchulungen.Count());
                // sende eine Mail für alle diese Schulungen und Speicher, das die Mail gesendet wurde
                if (ungeprüfteSchulungen.Count() > 0) {
                    var vorstand = Util.getVorstand(_context);
                    foreach (var schulung in ungeprüfteSchulungen) {
                        Task.WaitAll(
                            schulungRepository.SetGeprüftMailSent(schulung.SchulungGUID, true),
                            mailingHelper.GenerateAndSendGeprueftReminderMail(schulung, vorstand)
                        );
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            logger.Info("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}