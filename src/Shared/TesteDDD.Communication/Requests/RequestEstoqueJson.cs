using System.ComponentModel.DataAnnotations;

namespace TesteDDD.Communication.Requests;

public class RequestEstoqueJson
{
    [Required(ErrorMessage = "ProdutoId e obrigatorio.")]
    public Guid ProdutoId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantidade disponivel invalida.")]
    public int QuantidadeDisponivel { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantidade minima invalida.")]
    public int QuantidadeMinima { get; set; }
}
