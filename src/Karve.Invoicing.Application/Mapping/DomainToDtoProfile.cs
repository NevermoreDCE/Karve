using AutoMapper;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Domain.Entities;
using Karve.Invoicing.Domain.ValueObjects;

namespace Karve.Invoicing.Application.Mapping;

public class DomainToDtoProfile : Profile
{
    public DomainToDtoProfile()
    {
        CreateMap<Company, CompanyDto>();

        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value));

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.UnitPriceAmount, opt => opt.MapFrom(src => src.UnitPrice.Amount))
            .ForMember(dest => dest.UnitPriceCurrency, opt => opt.MapFrom(src => src.UnitPrice.Currency));

        CreateMap<Invoice, InvoiceDto>()
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : string.Empty));

        CreateMap<InvoiceLineItem, InvoiceLineItemDto>()
            .ForMember(dest => dest.UnitPriceAmount, opt => opt.MapFrom(src => src.UnitPrice.Amount))
            .ForMember(dest => dest.UnitPriceCurrency, opt => opt.MapFrom(src => src.UnitPrice.Currency));

        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Amount.Currency))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount));

        CreateMap<AppUser, UserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value));
    }
}