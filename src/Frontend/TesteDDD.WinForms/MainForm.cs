using System.Globalization;
using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;

namespace TesteDDD.WinForms;

public sealed class MainForm : Form
{
    private readonly ApiClient _apiClient = new("https://localhost:7192/");
    private readonly BindingSource _fornecedoresSource = new();
    private readonly BindingSource _produtosSource = new();
    private readonly BindingSource _estoquesSource = new();
    private readonly BindingSource _movimentacoesSource = new();
    private readonly BindingSource _ordensServicoSource = new();

    private readonly TextBox _baseUrlTextBox = new() { Text = "https://localhost:7192/" };
    private readonly Label _statusLabel = new() { Text = "Pronto.", AutoSize = true };

    private readonly DataGridView _fornecedoresGrid = CreateReadOnlyGrid();
    private readonly TextBox _fornecedorNomeTextBox = new();
    private readonly TextBox _fornecedorDocumentoTextBox = new();
    private readonly TextBox _fornecedorEmailTextBox = new();
    private readonly TextBox _fornecedorTelefoneTextBox = new();
    private Guid? _fornecedorSelecionadoId;

    private readonly DataGridView _produtosGrid = CreateReadOnlyGrid();
    private readonly TextBox _produtoNomeTextBox = new();
    private readonly NumericUpDown _produtoPrecoNumeric = new() { DecimalPlaces = 2, Maximum = 999999999, Minimum = 0.01M, ThousandsSeparator = true };
    private readonly ComboBox _produtoCategoriaCombo = CreateDropDown();
    private readonly ComboBox _produtoFornecedorCombo = CreateDropDown();
    private readonly List<ResponseCategoriaJson> _categorias = [];
    private readonly List<ResponseFornecedorJson> _fornecedores = [];
    private Guid? _produtoSelecionadoId;

    private readonly DataGridView _estoquesGrid = CreateReadOnlyGrid();
    private readonly DataGridView _movimentacoesGrid = CreateReadOnlyGrid();
    private readonly ComboBox _estoqueProdutoCombo = CreateDropDown();
    private readonly NumericUpDown _estoqueDisponivelNumeric = new() { Maximum = 999999, Minimum = 0 };
    private readonly NumericUpDown _estoqueMinimoNumeric = new() { Maximum = 999999, Minimum = 0 };
    private readonly ComboBox _movimentacaoProdutoCombo = CreateDropDown();
    private readonly NumericUpDown _movimentacaoQuantidadeNumeric = new() { Maximum = 999999, Minimum = 1, Value = 1 };
    private readonly TextBox _movimentacaoObservacaoTextBox = new();
    private readonly ComboBox _filtroMovimentacaoCombo = CreateDropDown(includeEmptyOption: true);

    private readonly DataGridView _ordensServicoGrid = CreateReadOnlyGrid();
    private readonly ComboBox _ordemClienteCombo = CreateDropDown();
    private readonly ComboBox _ordemProdutoCombo = CreateDropDown();
    private readonly TextBox _ordemDescricaoTextBox = new() { Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly DateTimePicker _ordemAberturaPicker = new() { Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker _ordemConclusaoPicker = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
    private readonly NumericUpDown _ordemValorNumeric = new() { DecimalPlaces = 2, Maximum = 999999999, Minimum = 0, ThousandsSeparator = true };
    private readonly ComboBox _ordemStatusCombo = CreateDropDown();
    private readonly List<ResponseClienteJson> _clientes = [];
    private Guid? _ordemSelecionadaId;

    public MainForm()
    {
        Text = "TesteDDD Desktop";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1280, 760);
        Width = 1400;
        Height = 860;

        _fornecedoresGrid.DataSource = _fornecedoresSource;
        _produtosGrid.DataSource = _produtosSource;
        _estoquesGrid.DataSource = _estoquesSource;
        _movimentacoesGrid.DataSource = _movimentacoesSource;
        _ordensServicoGrid.DataSource = _ordensServicoSource;

        _fornecedoresGrid.SelectionChanged += (_, _) => PopulateFornecedorForm();
        _produtosGrid.SelectionChanged += (_, _) => PopulateProdutoForm();
        _estoquesGrid.SelectionChanged += (_, _) => PopulateEstoqueForm();
        _ordensServicoGrid.SelectionChanged += (_, _) => PopulateOrdemServicoForm();

        BuildLayout();
        BindLookups();

        Shown += async (_, _) => await LoadAllAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(BuildHeaderPanel(), 0, 0);
        root.Controls.Add(BuildTabs(), 0, 1);
        root.Controls.Add(BuildStatusPanel(), 0, 2);

        Controls.Add(root);
    }

    private Control BuildHeaderPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12)
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "Backoffice WinForms",
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold)
        };

        var apiLabel = new Label
        {
            Text = "API",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(18, 8, 8, 0)
        };

        _baseUrlTextBox.Dock = DockStyle.Fill;

        var connectButton = new Button
        {
            Text = "Aplicar URL",
            AutoSize = true
        };
        connectButton.Click += async (_, _) => await ExecuteAsync("Atualizando URL da API", async () =>
        {
            _apiClient.SetBaseAddress(_baseUrlTextBox.Text.Trim());
            await LoadAllAsync();
        });

        var refreshButton = new Button
        {
            Text = "Atualizar tudo",
            AutoSize = true
        };
        refreshButton.Click += async (_, _) => await LoadAllAsync();

        panel.Controls.Add(title, 0, 0);
        panel.Controls.Add(apiLabel, 2, 0);
        panel.Controls.Add(_baseUrlTextBox, 1, 0);
        panel.Controls.Add(connectButton, 3, 0);
        panel.Controls.Add(refreshButton, 4, 0);

        return panel;
    }

    private Control BuildStatusPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 36,
            Padding = new Padding(4, 8, 4, 0)
        };

        _statusLabel.ForeColor = Color.DimGray;
        panel.Controls.Add(_statusLabel);
        return panel;
    }

    private Control BuildTabs()
    {
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill
        };

        tabs.TabPages.Add(CreateTabPage("Fornecedores", BuildFornecedoresTab()));
        tabs.TabPages.Add(CreateTabPage("Produtos", BuildProdutosTab()));
        tabs.TabPages.Add(CreateTabPage("Estoque", BuildEstoqueTab()));
        tabs.TabPages.Add(CreateTabPage("Ordens de Servico", BuildOrdensServicoTab()));

        return tabs;
    }

    private Control BuildFornecedoresTab()
    {
        var split = CreateSplitLayout();
        split.Panel1.Controls.Add(_fornecedoresGrid);
        split.Panel2.Controls.Add(BuildFornecedorForm());
        return split;
    }

    private Control BuildProdutosTab()
    {
        var split = CreateSplitLayout();
        split.Panel1.Controls.Add(_produtosGrid);
        split.Panel2.Controls.Add(BuildProdutoForm());
        return split;
    }

    private Control BuildEstoqueTab()
    {
        var container = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

        container.Controls.Add(BuildEstoqueForms(), 0, 0);

        var estoqueGroup = CreateGroup("Saldos");
        estoqueGroup.Controls.Add(_estoquesGrid);
        container.Controls.Add(estoqueGroup, 0, 1);

        var movimentacaoGroup = CreateGroup("Movimentacoes");
        movimentacaoGroup.Controls.Add(BuildMovimentacoesPanel());
        container.Controls.Add(movimentacaoGroup, 0, 2);

        return container;
    }

    private Control BuildOrdensServicoTab()
    {
        var split = CreateSplitLayout();
        split.Panel1.Controls.Add(_ordensServicoGrid);
        split.Panel2.Controls.Add(BuildOrdemServicoForm());
        return split;
    }

    private Control BuildFornecedorForm()
    {
        var panel = CreateFormPanel();

        panel.Controls.Add(CreateField("Nome", _fornecedorNomeTextBox));
        panel.Controls.Add(CreateField("Documento", _fornecedorDocumentoTextBox));
        panel.Controls.Add(CreateField("Email", _fornecedorEmailTextBox));
        panel.Controls.Add(CreateField("Telefone", _fornecedorTelefoneTextBox));
        panel.Controls.Add(CreateActionBar(
            ("Novo", (_, _) => ClearFornecedorForm()),
            ("Salvar", async (_, _) => await SaveFornecedorAsync()),
            ("Excluir", async (_, _) => await DeleteFornecedorAsync()),
            ("Atualizar lista", async (_, _) => await LoadFornecedoresAsync())));

        return WrapScrollable(panel);
    }

    private Control BuildProdutoForm()
    {
        var panel = CreateFormPanel();

        panel.Controls.Add(CreateField("Nome", _produtoNomeTextBox));
        panel.Controls.Add(CreateField("Preco", _produtoPrecoNumeric));
        panel.Controls.Add(CreateField("Categoria", _produtoCategoriaCombo));
        panel.Controls.Add(CreateField("Fornecedor", _produtoFornecedorCombo));
        panel.Controls.Add(CreateActionBar(
            ("Novo", (_, _) => ClearProdutoForm()),
            ("Salvar", async (_, _) => await SaveProdutoAsync()),
            ("Excluir", async (_, _) => await DeleteProdutoAsync()),
            ("Atualizar lista", async (_, _) => await LoadProdutosAsync())));

        return WrapScrollable(panel);
    }

    private Control BuildEstoqueForms()
    {
        var wrapper = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true
        };
        wrapper.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        wrapper.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        var saldoPanel = CreateFormPanel();
        saldoPanel.Controls.Add(CreateField("Produto", _estoqueProdutoCombo));
        saldoPanel.Controls.Add(CreateField("Qtd. disponivel", _estoqueDisponivelNumeric));
        saldoPanel.Controls.Add(CreateField("Qtd. minima", _estoqueMinimoNumeric));
        saldoPanel.Controls.Add(CreateActionBar(
            ("Limpar", (_, _) => ClearEstoqueForm()),
            ("Configurar saldo", async (_, _) => await ConfigurarEstoqueAsync()),
            ("Atualizar saldos", async (_, _) => await LoadEstoquesAsync())));

        var movimentoPanel = CreateFormPanel();
        movimentoPanel.Controls.Add(CreateField("Produto", _movimentacaoProdutoCombo));
        movimentoPanel.Controls.Add(CreateField("Quantidade", _movimentacaoQuantidadeNumeric));
        movimentoPanel.Controls.Add(CreateField("Observacao", _movimentacaoObservacaoTextBox, 90));
        movimentoPanel.Controls.Add(CreateActionBar(
            ("Entrada", async (_, _) => await MovimentarEstoqueAsync("entrada")),
            ("Saida", async (_, _) => await MovimentarEstoqueAsync("saida")),
            ("Ajuste", async (_, _) => await MovimentarEstoqueAsync("ajuste")),
            ("Atualizar mov.", async (_, _) => await LoadMovimentacoesAsync())));

        wrapper.Controls.Add(CreateGroup("Configuracao", WrapScrollable(saldoPanel)), 0, 0);
        wrapper.Controls.Add(CreateGroup("Movimento rapido", WrapScrollable(movimentoPanel)), 1, 0);

        return wrapper;
    }

    private Control BuildMovimentacoesPanel()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 0, 0, 8)
        };

        var filtroLabel = new Label { Text = "Filtrar por produto", AutoSize = true, Margin = new Padding(0, 10, 8, 0) };
        var filtrarButton = new Button { Text = "Aplicar filtro", AutoSize = true };
        filtrarButton.Click += async (_, _) => await LoadMovimentacoesAsync();
        var limparButton = new Button { Text = "Todos", AutoSize = true };
        limparButton.Click += async (_, _) =>
        {
            _filtroMovimentacaoCombo.SelectedIndex = 0;
            await LoadMovimentacoesAsync();
        };

        toolbar.Controls.Add(filtroLabel);
        toolbar.Controls.Add(_filtroMovimentacaoCombo);
        toolbar.Controls.Add(filtrarButton);
        toolbar.Controls.Add(limparButton);

        root.Controls.Add(toolbar, 0, 0);
        root.Controls.Add(_movimentacoesGrid, 0, 1);
        return root;
    }

    private Control BuildOrdemServicoForm()
    {
        var panel = CreateFormPanel();

        _ordemStatusCombo.Items.AddRange([
            new ComboItem(1, "Aberta"),
            new ComboItem(2, "Em andamento"),
            new ComboItem(3, "Concluida"),
            new ComboItem(4, "Cancelada")
        ]);
        _ordemStatusCombo.SelectedIndex = 0;

        panel.Controls.Add(CreateField("Cliente", _ordemClienteCombo));
        panel.Controls.Add(CreateField("Produto", _ordemProdutoCombo));
        panel.Controls.Add(CreateField("Descricao", _ordemDescricaoTextBox, 120));
        panel.Controls.Add(CreateField("Abertura", _ordemAberturaPicker));
        panel.Controls.Add(CreateField("Conclusao", _ordemConclusaoPicker));
        panel.Controls.Add(CreateField("Valor", _ordemValorNumeric));
        panel.Controls.Add(CreateField("Status", _ordemStatusCombo));
        panel.Controls.Add(CreateActionBar(
            ("Novo", (_, _) => ClearOrdemServicoForm()),
            ("Salvar", async (_, _) => await SaveOrdemServicoAsync()),
            ("Excluir", async (_, _) => await DeleteOrdemServicoAsync()),
            ("Atualizar lista", async (_, _) => await LoadOrdensServicoAsync())));

        return WrapScrollable(panel);
    }

    private async Task LoadAllAsync()
    {
        await ExecuteAsync("Carregando dados", async () =>
        {
            await LoadFornecedoresAsync();
            await LoadCategoriasAsync();
            await LoadClientesAsync();
            await LoadProdutosAsync();
            await LoadEstoquesAsync();
            await LoadMovimentacoesAsync();
            await LoadOrdensServicoAsync();
        });
    }

    private async Task LoadFornecedoresAsync()
    {
        _fornecedores.Clear();
        _fornecedores.AddRange(await _apiClient.GetFornecedoresAsync());
        _fornecedoresSource.DataSource = _fornecedores
            .Select(x => new
            {
                x.Id,
                x.Nome,
                x.Documento,
                x.Email,
                x.Telefone
            })
            .ToList();

        RebindFornecedorCombos();
    }

    private async Task LoadCategoriasAsync()
    {
        _categorias.Clear();
        _categorias.AddRange(await _apiClient.GetCategoriasAsync());
        RebindProdutoLookupCombos();
    }

    private async Task LoadClientesAsync()
    {
        _clientes.Clear();
        _clientes.AddRange(await _apiClient.GetClientesAsync());
        RebindOrdemServicoCombos();
    }

    private async Task LoadProdutosAsync()
    {
        var produtos = await _apiClient.GetProdutosAsync();
        _produtosSource.DataSource = produtos
            .Select(x => new
            {
                x.Id,
                x.Nome,
                x.Preco,
                x.CategoriaId,
                x.FornecedorId,
                Fornecedor = x.FornecedorNome
            })
            .ToList();

        _produtoCache = produtos;
        RebindProdutoCombos();
    }

    private async Task LoadEstoquesAsync()
    {
        var estoques = await _apiClient.GetEstoquesAsync();
        _estoquesSource.DataSource = estoques
            .Select(x => new
            {
                x.Id,
                x.ProdutoId,
                Produto = x.ProdutoNome,
                Disponivel = x.QuantidadeDisponivel,
                Minimo = x.QuantidadeMinima
            })
            .ToList();

        _estoqueCache = estoques;
    }

    private async Task LoadMovimentacoesAsync()
    {
        var produtoId = (_filtroMovimentacaoCombo.SelectedItem as ComboItem)?.Id;
        var movimentacoes = await _apiClient.GetMovimentacoesAsync(produtoId);
        _movimentacoesSource.DataSource = movimentacoes
            .OrderByDescending(x => x.DataMovimentacao)
            .Select(x => new
            {
                x.Id,
                x.ProdutoId,
                Produto = x.ProdutoNome,
                Tipo = x.TipoNome,
                x.Quantidade,
                Anterior = x.QuantidadeAnterior,
                Atual = x.QuantidadeAtual,
                x.Observacao,
                Data = x.DataMovimentacao.ToString("g", CultureInfo.GetCultureInfo("pt-BR"))
            })
            .ToList();
    }

    private async Task LoadOrdensServicoAsync()
    {
        var ordens = await _apiClient.GetOrdensServicoAsync();
        _ordensServicoSource.DataSource = ordens
            .Select(x => new
            {
                x.Id,
                x.ClienteId,
                Cliente = x.ClienteNome,
                x.ProdutoId,
                Produto = x.ProdutoNome,
                x.Descricao,
                Abertura = x.DataAbertura.ToString("d", CultureInfo.GetCultureInfo("pt-BR")),
                Conclusao = x.DataConclusao?.ToString("d", CultureInfo.GetCultureInfo("pt-BR")),
                x.ValorServico,
                Status = x.StatusNome
            })
            .ToList();

        _ordemServicoCache = ordens;
    }

    private List<ResponseProdutoJson> _produtoCache = [];
    private List<ResponseEstoqueJson> _estoqueCache = [];
    private List<ResponseOrdemServicoJson> _ordemServicoCache = [];

    private void RebindFornecedorCombos()
    {
        var items = _fornecedores.Select(x => new ComboItem(x.Id, x.Nome)).ToList();
        BindCombo(_produtoFornecedorCombo, items);
    }

    private void RebindProdutoLookupCombos()
    {
        var categoriaItems = _categorias.Select(x => new ComboItem(x.Id, x.Name)).ToList();
        BindCombo(_produtoCategoriaCombo, categoriaItems);
    }

    private void RebindProdutoCombos()
    {
        var items = _produtoCache.Select(x => new ComboItem(x.Id, x.Nome)).ToList();
        BindCombo(_estoqueProdutoCombo, items);
        BindCombo(_movimentacaoProdutoCombo, items);

        var filtroItems = _produtoCache.Select(x => new ComboItem(x.Id, x.Nome)).ToList();
        BindCombo(_filtroMovimentacaoCombo, filtroItems, includeEmptyOption: true);

        RebindOrdemServicoCombos();
    }

    private void RebindOrdemServicoCombos()
    {
        var clienteItems = _clientes.Select(x => new ComboItem(x.Id, x.Nome)).ToList();
        var produtoItems = _produtoCache.Select(x => new ComboItem(x.Id, x.Nome)).ToList();

        BindCombo(_ordemClienteCombo, clienteItems);
        BindCombo(_ordemProdutoCombo, produtoItems);
    }

    private void PopulateFornecedorForm()
    {
        if (_fornecedoresGrid.CurrentRow?.DataBoundItem is not null &&
            TryGetSelectedGuid(_fornecedoresGrid, "Id", out var id))
        {
            var fornecedor = _fornecedores.FirstOrDefault(x => x.Id == id);
            if (fornecedor is null)
            {
                return;
            }

            _fornecedorSelecionadoId = fornecedor.Id;
            _fornecedorNomeTextBox.Text = fornecedor.Nome;
            _fornecedorDocumentoTextBox.Text = fornecedor.Documento;
            _fornecedorEmailTextBox.Text = fornecedor.Email;
            _fornecedorTelefoneTextBox.Text = fornecedor.Telefone;
        }
    }

    private void PopulateProdutoForm()
    {
        if (!TryGetSelectedGuid(_produtosGrid, "Id", out var id))
        {
            return;
        }

        var produto = _produtoCache.FirstOrDefault(x => x.Id == id);
        if (produto is null)
        {
            return;
        }

        _produtoSelecionadoId = produto.Id;
        _produtoNomeTextBox.Text = produto.Nome;
        _produtoPrecoNumeric.Value = produto.Preco;
        SelectComboItem(_produtoCategoriaCombo, produto.CategoriaId);
        SelectComboItem(_produtoFornecedorCombo, produto.FornecedorId);
    }

    private void PopulateEstoqueForm()
    {
        if (!TryGetSelectedGuid(_estoquesGrid, "ProdutoId", out var produtoId))
        {
            return;
        }

        var estoque = _estoqueCache.FirstOrDefault(x => x.ProdutoId == produtoId);
        if (estoque is null)
        {
            return;
        }

        SelectComboItem(_estoqueProdutoCombo, estoque.ProdutoId);
        SelectComboItem(_movimentacaoProdutoCombo, estoque.ProdutoId);
        _estoqueDisponivelNumeric.Value = estoque.QuantidadeDisponivel;
        _estoqueMinimoNumeric.Value = estoque.QuantidadeMinima;
    }

    private void PopulateOrdemServicoForm()
    {
        if (!TryGetSelectedGuid(_ordensServicoGrid, "Id", out var id))
        {
            return;
        }

        var ordem = _ordemServicoCache.FirstOrDefault(x => x.Id == id);
        if (ordem is null)
        {
            return;
        }

        _ordemSelecionadaId = ordem.Id;
        SelectComboItem(_ordemClienteCombo, ordem.ClienteId);
        SelectComboItem(_ordemProdutoCombo, ordem.ProdutoId);
        _ordemDescricaoTextBox.Text = ordem.Descricao;
        _ordemAberturaPicker.Value = ordem.DataAbertura;
        _ordemConclusaoPicker.Checked = ordem.DataConclusao.HasValue;
        _ordemConclusaoPicker.Value = ordem.DataConclusao ?? DateTime.Today;
        _ordemValorNumeric.Value = ordem.ValorServico;
        SelectComboItem(_ordemStatusCombo, ordem.Status);
    }

    private async Task SaveFornecedorAsync()
    {
        var request = new RequestFornecedorJson
        {
            Nome = _fornecedorNomeTextBox.Text.Trim(),
            Documento = _fornecedorDocumentoTextBox.Text.Trim(),
            Email = _fornecedorEmailTextBox.Text.Trim(),
            Telefone = _fornecedorTelefoneTextBox.Text.Trim()
        };

        await ExecuteAsync("Salvando fornecedor", async () =>
        {
            await _apiClient.SaveFornecedorAsync(_fornecedorSelecionadoId, request);
            ClearFornecedorForm();
            await LoadFornecedoresAsync();
        });
    }

    private async Task DeleteFornecedorAsync()
    {
        if (!_fornecedorSelecionadoId.HasValue)
        {
            return;
        }

        await ExecuteAsync("Excluindo fornecedor", async () =>
        {
            await _apiClient.DeleteFornecedorAsync(_fornecedorSelecionadoId.Value);
            ClearFornecedorForm();
            await LoadFornecedoresAsync();
        });
    }

    private async Task SaveProdutoAsync()
    {
        var categoriaId = GetSelectedComboId(_produtoCategoriaCombo);
        var fornecedorId = GetSelectedComboId(_produtoFornecedorCombo);

        var request = new RequestProdutoJson
        {
            Nome = _produtoNomeTextBox.Text.Trim(),
            Preco = _produtoPrecoNumeric.Value,
            CategoriaId = categoriaId,
            FornecedorId = fornecedorId
        };

        await ExecuteAsync("Salvando produto", async () =>
        {
            await _apiClient.SaveProdutoAsync(_produtoSelecionadoId, request);
            ClearProdutoForm();
            await LoadProdutosAsync();
            await LoadEstoquesAsync();
            await LoadMovimentacoesAsync();
        });
    }

    private async Task DeleteProdutoAsync()
    {
        if (!_produtoSelecionadoId.HasValue)
        {
            return;
        }

        await ExecuteAsync("Excluindo produto", async () =>
        {
            await _apiClient.DeleteProdutoAsync(_produtoSelecionadoId.Value);
            ClearProdutoForm();
            await LoadProdutosAsync();
            await LoadEstoquesAsync();
            await LoadMovimentacoesAsync();
        });
    }

    private async Task ConfigurarEstoqueAsync()
    {
        var request = new RequestEstoqueJson
        {
            ProdutoId = GetSelectedComboId(_estoqueProdutoCombo),
            QuantidadeDisponivel = (int)_estoqueDisponivelNumeric.Value,
            QuantidadeMinima = (int)_estoqueMinimoNumeric.Value
        };

        await ExecuteAsync("Configurando estoque", async () =>
        {
            await _apiClient.ConfigurarEstoqueAsync(request);
            await LoadEstoquesAsync();
            await LoadMovimentacoesAsync();
        });
    }

    private async Task MovimentarEstoqueAsync(string tipo)
    {
        var request = new RequestMovimentacaoEstoqueJson
        {
            ProdutoId = GetSelectedComboId(_movimentacaoProdutoCombo),
            Quantidade = (int)_movimentacaoQuantidadeNumeric.Value,
            Observacao = _movimentacaoObservacaoTextBox.Text.Trim()
        };

        await ExecuteAsync($"Registrando {tipo} de estoque", async () =>
        {
            switch (tipo)
            {
                case "entrada":
                    await _apiClient.RegistrarEntradaAsync(request);
                    break;
                case "saida":
                    await _apiClient.RegistrarSaidaAsync(request);
                    break;
                default:
                    await _apiClient.AjustarEstoqueAsync(request);
                    break;
            }

            _movimentacaoQuantidadeNumeric.Value = 1;
            _movimentacaoObservacaoTextBox.Clear();
            await LoadEstoquesAsync();
            await LoadMovimentacoesAsync();
        });
    }

    private async Task SaveOrdemServicoAsync()
    {
        var request = new RequestOrdemServicoJson
        {
            ClienteId = GetSelectedComboId(_ordemClienteCombo),
            ProdutoId = GetSelectedComboId(_ordemProdutoCombo),
            Descricao = _ordemDescricaoTextBox.Text.Trim(),
            DataAbertura = _ordemAberturaPicker.Value.Date,
            DataConclusao = _ordemConclusaoPicker.Checked ? _ordemConclusaoPicker.Value.Date : null,
            ValorServico = _ordemValorNumeric.Value,
            Status = GetSelectedComboIntValue(_ordemStatusCombo)
        };

        await ExecuteAsync("Salvando ordem de servico", async () =>
        {
            await _apiClient.SaveOrdemServicoAsync(_ordemSelecionadaId, request);
            ClearOrdemServicoForm();
            await LoadOrdensServicoAsync();
        });
    }

    private async Task DeleteOrdemServicoAsync()
    {
        if (!_ordemSelecionadaId.HasValue)
        {
            return;
        }

        await ExecuteAsync("Excluindo ordem de servico", async () =>
        {
            await _apiClient.DeleteOrdemServicoAsync(_ordemSelecionadaId.Value);
            ClearOrdemServicoForm();
            await LoadOrdensServicoAsync();
        });
    }

    private void ClearFornecedorForm()
    {
        _fornecedorSelecionadoId = null;
        _fornecedorNomeTextBox.Clear();
        _fornecedorDocumentoTextBox.Clear();
        _fornecedorEmailTextBox.Clear();
        _fornecedorTelefoneTextBox.Clear();
    }

    private void ClearProdutoForm()
    {
        _produtoSelecionadoId = null;
        _produtoNomeTextBox.Clear();
        _produtoPrecoNumeric.Value = 0.01M;
        if (_produtoCategoriaCombo.Items.Count > 0) _produtoCategoriaCombo.SelectedIndex = 0;
        if (_produtoFornecedorCombo.Items.Count > 0) _produtoFornecedorCombo.SelectedIndex = 0;
    }

    private void ClearEstoqueForm()
    {
        if (_estoqueProdutoCombo.Items.Count > 0) _estoqueProdutoCombo.SelectedIndex = 0;
        _estoqueDisponivelNumeric.Value = 0;
        _estoqueMinimoNumeric.Value = 0;
    }

    private void ClearOrdemServicoForm()
    {
        _ordemSelecionadaId = null;
        if (_ordemClienteCombo.Items.Count > 0) _ordemClienteCombo.SelectedIndex = 0;
        if (_ordemProdutoCombo.Items.Count > 0) _ordemProdutoCombo.SelectedIndex = 0;
        _ordemDescricaoTextBox.Clear();
        _ordemAberturaPicker.Value = DateTime.Today;
        _ordemConclusaoPicker.Checked = false;
        _ordemValorNumeric.Value = 0;
        if (_ordemStatusCombo.Items.Count > 0) _ordemStatusCombo.SelectedIndex = 0;
    }

    private async Task ExecuteAsync(string action, Func<Task> operation)
    {
        try
        {
            SetStatus($"{action}...");
            UseWaitCursor = true;
            await operation();
            SetStatus($"{action} concluido.");
        }
        catch (Exception ex)
        {
            SetStatus($"{action} falhou.");
            MessageBox.Show(this, ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void SetStatus(string message)
    {
        _statusLabel.Text = $"{DateTime.Now:HH:mm:ss}  {message}";
    }

    private static SplitContainer CreateSplitLayout()
    {
        return new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 760
        };
    }

    private static DataGridView CreateReadOnlyGrid()
    {
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false
        };
    }

    private static ComboBox CreateDropDown(bool includeEmptyOption = false)
    {
        return new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 240,
            Tag = includeEmptyOption
        };
    }

    private static TabPage CreateTabPage(string title, Control content)
    {
        var page = new TabPage(title);
        content.Dock = DockStyle.Fill;
        page.Controls.Add(content);
        return page;
    }

    private static GroupBox CreateGroup(string title, Control? content = null)
    {
        var group = new GroupBox
        {
            Text = title,
            Dock = DockStyle.Fill,
            Padding = new Padding(12)
        };

        if (content is not null)
        {
            content.Dock = DockStyle.Fill;
            group.Controls.Add(content);
        }

        return group;
    }

    private static FlowLayoutPanel CreateFormPanel()
    {
        return new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Padding = new Padding(0)
        };
    }

    private static Control WrapScrollable(Control content)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };
        panel.Controls.Add(content);
        return panel;
    }

    private static Control CreateField(string label, Control input, int height = 34)
    {
        input.Width = 320;
        input.Height = height;

        var container = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 2,
            Width = 360,
            Height = height + 34,
            Margin = new Padding(0, 0, 0, 8)
        };
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        container.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 6)
        }, 0, 0);
        container.Controls.Add(input, 0, 1);
        return container;
    }

    private static Control CreateActionBar(params (string Text, EventHandler Handler)[] buttons)
    {
        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 0)
        };

        foreach (var (text, handler) in buttons)
        {
            var button = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 34
            };
            button.Click += handler;
            panel.Controls.Add(button);
        }

        return panel;
    }

    private static void BindCombo(ComboBox combo, List<ComboItem> items, bool includeEmptyOption = false)
    {
        var selectedId = (combo.SelectedItem as ComboItem)?.Id;
        var source = items.ToList();

        if (includeEmptyOption)
        {
            source.Insert(0, new ComboItem(null, "Todos"));
        }

        combo.DataSource = null;
        combo.DisplayMember = nameof(ComboItem.Text);
        combo.ValueMember = nameof(ComboItem.Id);
        combo.DataSource = source;

        if (selectedId.HasValue)
        {
            SelectComboItem(combo, selectedId.Value);
        }
    }

    private static void SelectComboItem(ComboBox combo, Guid id)
    {
        for (var i = 0; i < combo.Items.Count; i++)
        {
            if (combo.Items[i] is ComboItem item && item.Id == id)
            {
                combo.SelectedIndex = i;
                return;
            }
        }
    }

    private static void SelectComboItem(ComboBox combo, int id)
    {
        for (var i = 0; i < combo.Items.Count; i++)
        {
            if (combo.Items[i] is ComboItem comboItem && comboItem.IntValue == id)
            {
                combo.SelectedIndex = i;
                return;
            }
        }
    }

    private static Guid GetSelectedComboId(ComboBox combo)
    {
        if (combo.SelectedItem is ComboItem item && item.Id.HasValue)
        {
            return item.Id.Value;
        }

        throw new InvalidOperationException("Selecione um item.");
    }

    private static int GetSelectedComboIntValue(ComboBox combo)
    {
        if (combo.SelectedItem is ComboItem item)
        {
            return item.IntValue;
        }

        throw new InvalidOperationException("Selecione um item.");
    }

    private static bool TryGetSelectedGuid(DataGridView grid, string columnName, out Guid id)
    {
        id = Guid.Empty;

        if (grid.CurrentRow?.DataBoundItem is null)
        {
            return false;
        }

        var value = grid.CurrentRow.Cells[columnName].Value;
        return value is Guid guid && (id = guid) != Guid.Empty;
    }

    private void BindLookups()
    {
        _produtoPrecoNumeric.Value = 0.01M;
        _ordemAberturaPicker.Value = DateTime.Today;
        _ordemConclusaoPicker.Value = DateTime.Today;
    }

    private sealed class ComboItem
    {
        public ComboItem(Guid? id, string text)
        {
            Id = id;
            Text = text;
        }

        public ComboItem(int value, string text)
        {
            IntValue = value;
            Text = text;
        }

        public Guid? Id { get; }
        public int IntValue { get; }
        public string Text { get; }

        public override string ToString() => Text;
    }
}
