using System.ComponentModel.DataAnnotations;

namespace TesteDDD.Communication.Requests
{
    public class RequestProdutoJson
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [MaxLength(120, ErrorMessage = "Nome deve ter no máximo 120 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Range(0.01, 9999999999d, ErrorMessage = "Preço deve ser maior que zero.")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "CategoriaId é obrigatório.")]
        public Guid CategoriaId { get; set; }
    }
}
