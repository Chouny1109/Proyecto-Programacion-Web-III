using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.EF;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public interface IInvitacionService
    {
        void AgregarInvitacion(InvitacionPizarra invitacion);
        Task<InvitacionPizarra> ObtenerInvitacionPorCodigo(string codigo);
    }
    public class InvitacionService : IInvitacionService
    {
        private readonly ProyectoPizarraContext _context;
        public InvitacionService(ProyectoPizarraContext context)
        {
            _context = context;
        }
        public void AgregarInvitacion(InvitacionPizarra invitacion)
        {
            _context.InvitacionPizarras.Add(invitacion);
            _context.SaveChanges();
        }

        public Task<InvitacionPizarra> ObtenerInvitacionPorCodigo(string codigo)
        {
            return _context.InvitacionPizarras
                .FirstOrDefaultAsync(i => i.CodigoInvitacion == codigo);
        
    }
    }


}
