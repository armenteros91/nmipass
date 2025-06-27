using AutoMapper;
using Newtonsoft.Json;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;
using ThreeTP.Payment.Application.Helpers;
using ThreeTP.Payment.Domain.Entities.Nmi;
using ThreeTP.Payment.Domain.Entities.Payments;

namespace ThreeTP.Payment.Application.Mappings;

public class NmiMappingProfile : Profile
{
    public NmiMappingProfile()
    {
        // Map request DTO -> Request entity log 
        CreateMap<SaleTransactionRequestDto, NmiTransactionRequestLog>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId ?? "N/A"))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeTransaction))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .AfterMap((src, dest) =>
            {
                dest.PayloadJson = JsonConvert.SerializeObject(Utils.SanitizeRequestForLogging(src),
                    formatting: Formatting.Indented);
            }).ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

        //map Request DTo -> RespopnseLog log 
        CreateMap<NmiResponseDto, NmiTransactionResponseLog>()
            .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.TransactionId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Response))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.ResponseText))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.RawResponse, opt => opt.Ignore());
        // .AfterMap((src, dest) =>
        //     dest.RawResponse = JsonConvert.SerializeObject(src, formatting: Formatting.Indented));
        //

        // CreateMap<NmiResponseDto, TransactionResponse>();
        CreateMap<NmiResponseDto, TransactionResponse>()
            .ForMember(dest => dest.ResponseText, opt => opt.MapFrom(src => src.ResponseText))
            .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.TransactionId))
            .ForMember(dest => dest.AuthCode, opt => opt.MapFrom(src => src.AuthCode))
            .ForMember(dest => dest.AvsResponse, opt => opt.MapFrom(src => src.AvsResponse))
            .ForMember(dest => dest.CvvResponse, opt => opt.MapFrom(src => src.CvvResponse))
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.ResponseCode, opt => opt.MapFrom(src => src.ResponseCode))
            .ForMember(dest => dest.EmvAuthResponseData, opt => opt.MapFrom(src => src.EmvAuthResponseData))
            .ForMember(dest => dest.CustomerVaultId, opt => opt.MapFrom(src => src.CustomerVaultId))
            .ForMember(dest => dest.KountScore, opt => opt.MapFrom(src => src.KountScore))
            .ForMember(dest => dest.MerchantAdviceCode, opt => opt.MapFrom(src => src.MerchantAdviceCode))
            .ForMember(dest => dest.Response, opt => opt.Ignore())
            .AfterMap((src, dest) => { dest.Response = int.TryParse(src.Response, out var result) ? result : 0; });

        //// Mapear DTO de respuesta a entidad incluyendo el vínculo con Transactions
        CreateMap<(NmiResponseDto dto, Guid transactionsId), TransactionResponse>()
            .ForMember(dest => dest.Response, opt => opt.MapFrom(src => src.dto.Response))
            .ForMember(dest => dest.ResponseText, opt => opt.MapFrom(src => src.dto.ResponseText))
            .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.dto.TransactionId))
            .ForMember(dest => dest.AuthCode, opt => opt.MapFrom(src => src.dto.AuthCode))
            .ForMember(dest => dest.AvsResponse, opt => opt.MapFrom(src => src.dto.AvsResponse))
            .ForMember(dest => dest.CvvResponse, opt => opt.MapFrom(src => src.dto.CvvResponse))
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.dto.OrderId))
            .ForMember(dest => dest.ResponseCode, opt => opt.MapFrom(src => src.dto.ResponseCode))
            .ForMember(dest => dest.EmvAuthResponseData, opt => opt.MapFrom(src => src.dto.EmvAuthResponseData))
            .ForMember(dest => dest.CustomerVaultId, opt => opt.MapFrom(src => src.dto.CustomerVaultId))
            .ForMember(dest => dest.KountScore, opt => opt.MapFrom(src => src.dto.KountScore))
            .ForMember(dest => dest.MerchantAdviceCode, opt => opt.MapFrom(src => src.dto.MerchantAdviceCode))
            .ForMember(dest => dest.FkTransactionsId, opt => opt.MapFrom(src => src.transactionsId));


        // Guarda el raw JSON request.
        CreateMap<CreateTransactionRequestDto, Transactions>()
            .ForMember(dest => dest.TransactionsId, opt => opt.MapFrom(src => src.TraceId)) // Generar nuevo GUID
            .ForMember(dest => dest.paymentTransactionId,opt => opt.MapFrom(src=>src.PaymentTransactionsId))
            .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId))
            .ForMember(dest => dest.TraceId, opt => opt.MapFrom(src => src.TraceId))
            .ForMember(dest=> dest.ResponseCode, opt => opt.MapFrom(src=>src.responseCode))
            .ForMember(dest => dest.TypeTransactionsId, opt => opt.MapFrom(src => src.TypeTransactionsId))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow)) // Fecha actual UTC
            .ForMember(dest => dest.CreatedBy,
                opt => opt.MapFrom(src => src.CreatedBy ?? "system")); // Valor por defecto

        //map desde dto UpdateTransactionRequestDTo -> transaction Entitity
        CreateMap<UpdateTransactionRequestDto, Transactions>()
            .ForMember(dest => dest.LastModifiedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy));


        CreateMap<(string response, string requestId, string? transactionId, string status, string message),
                NmiTransactionResponseLog>()
            .ForMember(dest => dest.RawResponse, opt => opt.MapFrom(src => src.response))
            .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.transactionId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.message))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .AfterMap((src, dest) =>
            {
                if (!Guid.TryParse(src.requestId, out var parsedGuid))
                {
                    throw new FormatException($"El requestId '{src.requestId}' no tiene un formato válido de GUID.");
                }

                dest.RequestId = parsedGuid;
            });
    }
}