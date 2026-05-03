using System.ComponentModel.DataAnnotations;

namespace TesteDDD.Communication.Requests;

public class RequestFornecedorJson
{
    [Required(ErrorMessage = "Nome e obrigatorio.")]
    [MaxLength(150, ErrorMessage = "Nome deve ter no maximo 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Documento e obrigatorio.")]
    [MaxLength(20, ErrorMessage = "Documento deve ter no maximo 20 caracteres.")]
    public string Documento { get; set; } = string.Empty;

    [MaxLength(150, ErrorMessage = "Email deve ter no maximo 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "Telefone deve ter no maximo 20 caracteres.")]
    public string Telefone { get; set; } = string.Empty;
}
