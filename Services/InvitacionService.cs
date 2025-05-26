using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.EF;

namespace Services
{
    public interface IInvitacionService
    {
        void AgregarInvitacion(InvitacionPizarra invitacion);
        Task<InvitacionPizarra> ObtenerInvitacionPorCodigo(string codigo);
    }
    public class InvitacionService : IInvitacionService
    {
        public void AgregarInvitacion(InvitacionPizarra invitacion)
        {
            throw new NotImplementedException();
        }

        public Task<InvitacionPizarra> ObtenerInvitacionPorCodigo(string codigo)
        {
            throw new NotImplementedException();
        }
    }


}
