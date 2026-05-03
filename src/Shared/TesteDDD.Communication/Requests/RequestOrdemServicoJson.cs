using System.ComponentModel.DataAnnotations;

namespace TesteDDD.Communication.Requests;

public class RequestOrdemServicoJson
{
    [Required(ErrorMessage = "ClienteId e obrigatorio.")]
    public Guid ClienteId { get; set; }

    [Required(ErrorMessage = "ProdutoId e obrigatorio.")]
    public Guid ProdutoId { get; set; }

    [Required(ErrorMessage = "Descricao e obrigatoria.")]
    [MaxLength(500, ErrorMessage = "Descricao deve ter no maximo 500 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data de abertura e obrigatoria.")]
    public DateTime DataAbertura { get; set; }

    public DateTime? DataConclusao { get; set; }

    [Range(0, 9999999999d, ErrorMessage = "Valor do servico invalido.")]
    public decimal ValorServico { get; set; }

    public int Status { get; set; } = 1;
}
