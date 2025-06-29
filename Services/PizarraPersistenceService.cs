﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Services
{
    public class PizarraPersistenceService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITrazoMemoryService _trazoMemoryService;
        private readonly ITextoMemoryService _textoMemoryService;
        private readonly ILogger<PizarraPersistenceService> _logger;

        public PizarraPersistenceService(IServiceScopeFactory scopeFactory, ITrazoMemoryService trazoService,
            ITextoMemoryService textoMemoryService,
            ILogger<PizarraPersistenceService> logger)
        {
            _scopeFactory = scopeFactory;
            _trazoMemoryService = trazoService;
            _textoMemoryService = textoMemoryService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    //persistir trazos a base de datos 
                    {
                        var pizarraService = scope.ServiceProvider.GetRequiredService<IPizarraService>();

                        await pizarraService.PersistirTrazosBD(_trazoMemoryService.ObtenerTodas());
                        _logger.LogInformation("Trazos guardados en DB a las {Hora}", DateTime.Now);

                        //persistir texto a base de datos
                        await pizarraService.PersistirTextosBD(_textoMemoryService.ObtenerTodas());
                        _logger.LogInformation("Textos guardados en DB a las {Hora}", DateTime.Now);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar trazos");
                }
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}
