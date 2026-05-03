using AutoMapper;
using TesteDDD.Communication.Responses;
using TesteDDD.Domain.Entities;

namespace TesteDDD.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Produto, ResponseProdutoJson>()
            .ForMember(dest => dest.FornecedorNome, opt => opt.MapFrom(src => src.Fornecedor.Nome));

        CreateMap<Categoria, ResponseCategoriaJson>();
        CreateMap<Cliente, ResponseClienteJson>();
        CreateMap<Fornecedor, ResponseFornecedorJson>();

        CreateMap<OrdemServico, ResponseOrdemServicoJson>()
            .ForMember(dest => dest.ClienteNome, opt => opt.MapFrom(src => src.Cliente.Nome))
            .ForMember(dest => dest.ProdutoNome, opt => opt.MapFrom(src => src.Produto.Nome))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
            .ForMember(dest => dest.StatusNome, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<Estoque, ResponseEstoqueJson>()
            .ForMember(dest => dest.ProdutoNome, opt => opt.MapFrom(src => src.Produto.Nome));

        CreateMap<MovimentacaoEstoque, ResponseMovimentacaoEstoqueJson>()
            .ForMember(dest => dest.ProdutoNome, opt => opt.MapFrom(src => src.Produto.Nome))
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => (int)src.Tipo))
            .ForMember(dest => dest.TipoNome, opt => opt.MapFrom(src => src.Tipo.ToString()));

        CreateMap<NotaFiscal, ResponseNotaFiscalJson>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
            .ForMember(dest => dest.StatusNome, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
