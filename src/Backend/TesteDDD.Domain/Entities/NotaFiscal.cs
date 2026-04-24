namespace TesteDDD.Domain.Entities;

public class NotaFiscal
{
    public Guid Id { get; private set; }
    public Guid VendasId { get; private set; }
    public Vendas Vendas { get; private set; } = null!;
    
    public int Serie { get; private set; }
    public int Numero { get; private set; }
    public string ChaveAcesso { get; private set; } = string.Empty;
    
    public DateTime DataEmissao { get; private set; }
    public DateTime? DataAutorizacao { get; private set; }
    
    public NotaFiscalStatus Status { get; private set; }
    public string ProtocoloAutorizacao { get; private set; } = string.Empty;
    
    public string XmlAutorizado { get; private set; } = string.Empty;
    public string DanfeUrl { get; private set; } = string.Empty;
    
    public string? MensagemErro { get; private set; }

    protected NotaFiscal()
    {
    }

    public NotaFiscal(Guid vendasId, int serie, int numero)
    {
        Id = Guid.NewGuid();
        VendasId = vendasId;
        Serie = serie;
        Numero = numero;
        DataEmissao = DateTime.UtcNow;
        Status = NotaFiscalStatus.Rascunho;
    }

    public void AtualizarChaveAcesso(string chaveAcesso)
    {
        if (string.IsNullOrWhiteSpace(chaveAcesso) || chaveAcesso.Length != 44)
            throw new ArgumentException("Chave de acesso inválida.", nameof(chaveAcesso));
        
        ChaveAcesso = chaveAcesso;
    }

    public void MarcarComoPendente()
    {
        Status = NotaFiscalStatus.Pendente;
    }

    public void MarcarComoAutorizada(string protocoloAutorizacao, string xmlAutorizado)
    {
        if (string.IsNullOrWhiteSpace(protocoloAutorizacao))
            throw new ArgumentException("Protocolo de autorização é obrigatório.", nameof(protocoloAutorizacao));
        
        Status = NotaFiscalStatus.Autorizada;
        DataAutorizacao = DateTime.UtcNow;
        ProtocoloAutorizacao = protocoloAutorizacao;
        XmlAutorizado = xmlAutorizado;
        MensagemErro = null;
    }

    public void MarcarComoRejeitada(string mensagemErro)
    {
        if (string.IsNullOrWhiteSpace(mensagemErro))
            throw new ArgumentException("Mensagem de erro é obrigatória.", nameof(mensagemErro));
        
        Status = NotaFiscalStatus.Rejeitada;
        MensagemErro = mensagemErro;
    }

    public void MarcarComoCancelada()
    {
        if (Status != NotaFiscalStatus.Autorizada)
            throw new InvalidOperationException("Apenas notas autorizadas podem ser canceladas.");
        
        Status = NotaFiscalStatus.Cancelada;
    }

    public void DefinirDanfeUrl(string danfeUrl)
    {
        if (string.IsNullOrWhiteSpace(danfeUrl))
            throw new ArgumentException("URL da DANFE é obrigatória.", nameof(danfeUrl));
        
        DanfeUrl = danfeUrl;
    }
}

public enum NotaFiscalStatus
{
    Rascunho = 1,
    Pendente = 2,
    Autorizada = 3,
    Rejeitada = 4,
    Cancelada = 5,
    Inutilizada = 6
}
