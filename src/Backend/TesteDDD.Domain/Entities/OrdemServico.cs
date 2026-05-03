namespace TesteDDD.Domain.Entities;

public class OrdemServico
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public Cliente Cliente { get; private set; } = null!;
    public Guid ProdutoId { get; private set; }
    public Produto Produto { get; private set; } = null!;
    public string Descricao { get; private set; } = string.Empty;
    public DateTime DataAbertura { get; private set; }
    public DateTime? DataConclusao { get; private set; }
    public decimal ValorServico { get; private set; }
    public OrdemServicoStatus Status { get; private set; }

    protected OrdemServico()
    {
    }

    public OrdemServico(Guid clienteId, Guid produtoId, string descricao, DateTime dataAbertura, decimal valorServico)
    {
        Id = Guid.NewGuid();
        Status = OrdemServicoStatus.Aberta;
        Update(clienteId, produtoId, descricao, dataAbertura, valorServico);
    }

    public void Update(Guid clienteId, Guid produtoId, string descricao, DateTime dataAbertura, decimal valorServico)
    {
        if (clienteId == Guid.Empty)
            throw new ArgumentException("ClienteId e obrigatorio.", nameof(clienteId));
        if (produtoId == Guid.Empty)
            throw new ArgumentException("ProdutoId e obrigatorio.", nameof(produtoId));
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descricao da ordem de servico e obrigatoria.", nameof(descricao));
        if (dataAbertura == default)
            throw new ArgumentException("Data de abertura e obrigatoria.", nameof(dataAbertura));
        if (valorServico < 0)
            throw new ArgumentException("Valor do servico nao pode ser negativo.", nameof(valorServico));

        ClienteId = clienteId;
        ProdutoId = produtoId;
        Descricao = descricao;
        DataAbertura = dataAbertura;
        ValorServico = valorServico;
    }

    public void Iniciar()
    {
        if (Status != OrdemServicoStatus.Aberta)
            throw new InvalidOperationException("A ordem de servico so pode ser iniciada quando estiver aberta.");

        Status = OrdemServicoStatus.EmAndamento;
    }

    public void Concluir(DateTime dataConclusao)
    {
        if (Status == OrdemServicoStatus.Cancelada)
            throw new InvalidOperationException("Nao e possivel concluir uma ordem de servico cancelada.");
        if (dataConclusao == default)
            throw new ArgumentException("Data de conclusao e obrigatoria.", nameof(dataConclusao));

        Status = OrdemServicoStatus.Concluida;
        DataConclusao = dataConclusao;
    }

    public void Cancelar()
    {
        if (Status == OrdemServicoStatus.Concluida)
            throw new InvalidOperationException("Nao e possivel cancelar uma ordem de servico concluida.");

        Status = OrdemServicoStatus.Cancelada;
    }
}

public enum OrdemServicoStatus
{
    Aberta = 1,
    EmAndamento = 2,
    Concluida = 3,
    Cancelada = 4
}
