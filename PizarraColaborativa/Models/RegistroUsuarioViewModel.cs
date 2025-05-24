using System.ComponentModel.DataAnnotations;

namespace PizarraColaborativa.Models
{
    public class RegistroUsuarioViewModel
    {
            [Required(ErrorMessage ="Campo requerido")]
            [EmailAddress]
            public string ?Email { get; set; }

            [Required(ErrorMessage = "Campo requerido")]
            [MinLength(4, ErrorMessage = "Minimo 4 caracteres")]
            public string? Password { get; set; }

            [Required(ErrorMessage = "Campo requerido")]
            public string? Rol { get; set; }
        }

    }

