using AutoMapper;
using TesteDDD.Communication.Requests;
using TesteDDD.Communication.Responses;
using TesteDDD.Domain.Entities;

namespace TesteDDD.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Produto
        CreateMap<Produto, ResponseProdutoJson>();
        CreateMap<RequestProdutoJson, Produto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Categoria, opt => opt.Ignore());

        // Categoria
        CreateMap<Categoria, ResponseCategoriaJson>();
        CreateMap<RequestCategoriaJson, Categoria>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Produtos, opt => opt.Ignore());

        // Cliente
        CreateMap<Cliente, ResponseClienteJson>();
        CreateMap<RequestClienteJson, Cliente>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
