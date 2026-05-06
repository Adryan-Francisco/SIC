using System.ComponentModel.DataAnnotations;

namespace TesteDDD.Communication.Requests
{
    public class RequestClienteJson
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [MaxLength(150, ErrorMessage = "Nome deve ter no máximo 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Endereço é obrigatório.")]
        [MinLength(5, ErrorMessage = "Endereço deve ter no mínimo 5 caracteres.")]
        [MaxLength(250, ErrorMessage = "Endereço deve ter no máximo 250 caracteres.")]
        public string Endereco { get; set; } = string.Empty;

        [Required(ErrorMessage = "CEP é obrigatório.")]
        [RegularExpression(@"^(\d{8,9}|\d{5}-\d{3})$", ErrorMessage = "CEP inválido. Use 12345-678, 12345678 ou 123456789.")]
        public string Cep { get; set; } = string.Empty;
    }
}
