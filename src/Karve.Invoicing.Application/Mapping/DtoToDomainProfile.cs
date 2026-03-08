using AutoMapper;
using Karve.Invoicing.Application.DTOs;
using Karve.Invoicing.Domain.Entities;
using Karve.Invoicing.Domain.ValueObjects;

namespace Karve.Invoicing.Application.Mapping;

public class DtoToDomainProfile : Profile
{
    public DtoToDomainProfile()
    {
        CreateMap<CreateCompanyRequest, Company>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()));

        CreateMap<UpdateCompanyRequest, Company>();

        CreateMap<CreateCustomerRequest, Customer>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => new EmailAddress(src.Email)));

        CreateMap<UpdateCustomerRequest, Customer>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => new EmailAddress(src.Email)));

        CreateMap<CreateProductRequest, Product>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => new Money(src.UnitPriceAmount, src.UnitPriceCurrency)));

        CreateMap<UpdateProductRequest, Product>()
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => new Money(src.UnitPriceAmount, src.UnitPriceCurrency)));

        CreateMap<CreateInvoiceRequest, Invoice>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()));

        CreateMap<UpdateInvoiceRequest, Invoice>();

        CreateMap<CreateInvoiceLineItemRequest, InvoiceLineItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => new Money(src.UnitPriceAmount, src.UnitPriceCurrency)));

        CreateMap<UpdateInvoiceLineItemRequest, InvoiceLineItem>()
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => new Money(src.UnitPriceAmount, src.UnitPriceCurrency)));

        CreateMap<CreatePaymentRequest, Payment>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => new Money(src.Amount, src.Currency)));

        CreateMap<UpdatePaymentRequest, Payment>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => new Money(src.Amount, src.Currency)));

        CreateMap<CreateUserRequest, AppUser>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => new EmailAddress(src.Email)));

        CreateMap<UpdateUserRequest, AppUser>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => new EmailAddress(src.Email)));
    }
}