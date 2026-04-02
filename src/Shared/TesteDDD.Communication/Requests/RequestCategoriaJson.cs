using System.ComponentModel.DataAnnotations;

namespace TesteDDD.Communication.Requests
{
    public class RequestCategoriaJson
    {
        [Required(ErrorMessage = "Nome e obrigatorio.")]
        [MaxLength(120, ErrorMessage = "Nome deve ter no maximo 120 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Descricao e obrigatoria.")]
        [MaxLength(500, ErrorMessage = "Descricao deve ter no maximo 500 caracteres.")]
        public string Descricao { get; set; } = string.Empty;
    }
}
