using System.ComponentModel.DataAnnotations;

namespace PizarraColaborativa.Models
{
    public class RegistroUsuarioViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [MinLength(3, ErrorMessage = "El nombre de usuario debe tener al menos 3 caracteres.")]
        [MaxLength(20, ErrorMessage = "El nombre de usuario no puede superar los 20 caracteres.")]
        [Display(Name = "Nombre de usuario")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "El campo correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debes ingresar un correo electrónico válido.")]
        [Display(Name = "Correo electrónico")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(4, ErrorMessage = "La contraseña debe tener al menos 4 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Debes seleccionar un rol.")]
        [Display(Name = "Rol del usuario")]
        public string? Rol { get; set; }
    }
}
