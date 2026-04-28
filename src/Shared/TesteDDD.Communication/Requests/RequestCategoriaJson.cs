using System.ComponentModel.DataAnnotations;

namespace TesteDDD.Communication.Requests
{
    public class RequestCategoriaJson
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [MaxLength(120, ErrorMessage = "Nome deve ter no máximo 120 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Descrição é obrigatória.")]
        [MinLength(5, ErrorMessage = "Descrição deve ter no mínimo 5 caracteres.")]
        [MaxLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres.")]
        public string Descricao { get; set; } = string.Empty;
    }
}
