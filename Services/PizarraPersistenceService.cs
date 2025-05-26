using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.EF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Services
{
    public class PizarraPersistenceService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TrazoMemoryService _memoryService;
        private readonly ILogger<PizarraPersistenceService> _logger;    

        public PizarraPersistenceService(IServiceProvider serviceProvider, TrazoMemoryService memoryService, ILogger<PizarraPersistenceService> logger)
        {
            _serviceProvider = serviceProvider;
            _memoryService = memoryService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ProyectoPizarraContext>();
                    
                    foreach(var (pizarraId,trazos) in _memoryService.ObtenerTodas())
                    {

                        var pizarraguid = Guid.Parse(pizarraId);
                        var existentes = context.Trazos.Where(t => t.PizarraId == pizarraguid);
                        context.Trazos.RemoveRange(existentes);

                        foreach(var trazo in trazos)
                        {
                            trazo.Id = 0;
                            trazo.PizarraId = Guid.Parse(pizarraId);
                            context.Trazos.Add(trazo);
                        }
                    }
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Trazos guardados en DB a las {Hora}", DateTime.Now);
                
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar trazos");
                }
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);

            }
        }
    }
}
