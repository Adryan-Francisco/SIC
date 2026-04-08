using System.ComponentModel.DataAnnotations;

namespace TesteDDD.Communication.Requests
{
    public class RequestClienteJson
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [MinLength(3, ErrorMessage = "Nome deve ter no mínimo 3 caracteres.")]
        [MaxLength(150, ErrorMessage = "Nome deve ter no máximo 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Endereço é obrigatório.")]
        [MinLength(5, ErrorMessage = "Endereço deve ter no mínimo 5 caracteres.")]
        [MaxLength(250, ErrorMessage = "Endereço deve ter no máximo 250 caracteres.")]
        public string Endereco { get; set; } = string.Empty;

        [Required(ErrorMessage = "CEP é obrigatório.")]
        [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "CEP inválido. Use 12345-678 ou 12345678.")]
        public string Cep { get; set; } = string.Empty;
    }
}
