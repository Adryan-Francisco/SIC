using System.ComponentModel.DataAnnotations;

namespace TesteDDD.Communication.Requests;

public class RequestProdutoJson
{
    [Required(ErrorMessage = "Nome e obrigatorio.")]
    [MaxLength(120, ErrorMessage = "Nome deve ter no maximo 120 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Range(0.01, 9999999999d, ErrorMessage = "Preco deve ser maior que zero.")]
    public decimal Preco { get; set; }

    [Required(ErrorMessage = "CategoriaId e obrigatorio.")]
    public Guid CategoriaId { get; set; }

    [Required(ErrorMessage = "FornecedorId e obrigatorio.")]
    public Guid FornecedorId { get; set; }
}
