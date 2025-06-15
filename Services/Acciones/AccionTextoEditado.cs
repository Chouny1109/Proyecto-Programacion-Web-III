using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.EF;
using Microsoft.AspNetCore.SignalR;
using Services.Acciones.Interfaces;

namespace Services.Acciones
{
   
        public class AccionTextoEditado : IAccionPizarra
        {
            private readonly string _textoId;
            private readonly string _contenidoAnterior;
            private readonly string _contenidoNuevo;
            private readonly int TamanioAnterior;
            private readonly int TamanioNuevo;
            private readonly string ColorAnterior;
            private readonly string ColorNuevo;



        public AccionTextoEditado(string textoId, string contenidoAnterior, string contenidoNuevo, string? colorAnterior, 
            string colorNuevo, int tamanioAnterior,int tamanioNuevo)
            {
                _textoId = textoId;
                _contenidoAnterior = contenidoAnterior;
                _contenidoNuevo = contenidoNuevo;
            TamanioAnterior = tamanioAnterior;
            TamanioNuevo = tamanioNuevo;
            ColorAnterior = colorAnterior ?? string.Empty;
            ColorNuevo = colorNuevo ?? string.Empty;
        }

       
        public Task Deshacer(string pizarraId, IHubCallerClients clients, ITrazoMemoryService trazoService, ITextoMemoryService textoService)
            {
                var texto = textoService.ObtenerTextoPorIdEnMemoria(pizarraId, _textoId);
                if (texto != null)
                {
                    texto.Contenido = _contenidoAnterior;
                texto.Tamano = TamanioAnterior;
                texto.Color = ColorAnterior;
                textoService.EditarTextoEnPizarra(texto, pizarraId);
                    return clients.Group(pizarraId).SendAsync("TextoActualizado", texto);
                }
                return Task.CompletedTask;
            }

            public Task Rehacer(string pizarraId, IHubCallerClients clients, ITrazoMemoryService trazoService, ITextoMemoryService textoService)
            {
                var texto = textoService.ObtenerTextoPorIdEnMemoria(pizarraId, _textoId);
                if (texto != null)
                {
                    texto.Contenido = _contenidoNuevo;
                texto.Tamano = TamanioNuevo;
                texto.Color = ColorNuevo;
                textoService.EditarTextoEnPizarra(texto, pizarraId);
                    return clients.Group(pizarraId).SendAsync("TextoActualizado", texto);
                }
                return Task.CompletedTask;
            }
        }
    }
    
