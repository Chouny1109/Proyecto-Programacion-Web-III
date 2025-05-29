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
      private readonly IPizarraService _pizarraService;
        private readonly TrazoMemoryService _trazoMemoryService;
        private readonly TextoMemoryService _textoMemoryService;
        private readonly ILogger<PizarraPersistenceService> _logger;    

        public PizarraPersistenceService(IPizarraService pizarraService, TrazoMemoryService trazoService,
            TextoMemoryService textoMemoryService, ILogger<PizarraPersistenceService> logger)
        {
            _pizarraService = pizarraService;
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
                    foreach(var (pizarraId,trazos) in _trazoMemoryService.ObtenerTodas())
                    {

                        var pizarraguid = Guid.Parse(pizarraId);
                        //Obtener trazos existentes de una pizarra
                        //Borrar trazos existentes
                        var existentes = _pizarraService.ObtenerTrazosDeUnaPizarra(pizarraguid);
                        _pizarraService.BorrarTrazosExistentesPizarra(existentes);

                        foreach (var trazo in trazos)
                        {
                            trazo.Id = 0;
                            trazo.PizarraId = Guid.Parse(pizarraId);
                            _pizarraService.AgregarTrazo(trazo);
                        }
                    }
                    foreach (var (pizaraId,textos) in _textoMemoryService)
                    {
                       
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
