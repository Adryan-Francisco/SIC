using System.ComponentModel.DataAnnotations;

namespace TesteDDD.Communication.Requests
{
    public class RequestClienteJson
    {
        [Required(ErrorMessage = "Nome e obrigatorio.")]
        [MaxLength(150, ErrorMessage = "Nome deve ter no maximo 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Endereco e obrigatorio.")]
        [MaxLength(250, ErrorMessage = "Endereco deve ter no maximo 250 caracteres.")]
        public string Endereco { get; set; } = string.Empty;

        [Required(ErrorMessage = "CEP e obrigatorio.")]
        [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "CEP invalido. Use 12345-678 ou 12345678.")]
        public string Cep { get; set; } = string.Empty;

    }
}
