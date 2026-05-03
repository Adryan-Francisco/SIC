using System.ComponentModel.DataAnnotations;

namespace TesteDDD.Communication.Requests;

public class RequestMovimentacaoEstoqueJson
{
    [Required(ErrorMessage = "ProdutoId e obrigatorio.")]
    public Guid ProdutoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; }

    [MaxLength(250, ErrorMessage = "Observacao deve ter no maximo 250 caracteres.")]
    public string Observacao { get; set; } = string.Empty;
}
