using System.Text.Json;
using AutoMapper;
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
        // 📝 Map request DTO -> Request Log BaseEntity
        CreateMap<BaseTransactionRequestDto, NmiTransactionRequestLog>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId ?? "N/A"))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeTransaction))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            // .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            // .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            // .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            // .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            // .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
            // .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
            // .ForMember(dest => dest.CardHolderAuth, opt => opt.MapFrom(src => src.CardHolderAuth))
            // .ForMember(dest => dest.ThreeDsVersion, opt => opt.MapFrom(src => src.ThreeDsVersion))
            // .ForMember(dest => dest.DirectoryServerId, opt => opt.MapFrom(src => src.DirectoryServerId))
            // .ForMember(dest => dest.TransactionSessionId, opt => opt.MapFrom(src => src.TransactionSessionId))
            // .ForMember(dest => dest.PaymentType, opt => opt.MapFrom(src => src.PaymentType))
            // .ForMember(dest => dest.CheckAccount, opt => opt.MapFrom(src => src.CheckAccount))
            // .ForMember(dest => dest.CheckName, opt => opt.MapFrom(src => src.CheckName))
            // .ForMember(dest => dest.CheckAba, opt => opt.MapFrom(src => src.CheckAba))
            // .ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => src.AccountType))
            // .ForMember(dest => dest.AccountHolderType, opt => opt.MapFrom(src => src.AccountHolderType))
            // .ForMember(dest => dest.SurchargeAmount, opt => opt.MapFrom(src => src.SurchargeAmount))
            // .ForMember(dest => dest.ConvenienceFee, opt => opt.MapFrom(src => src.ConvenienceFee))
            // .ForMember(dest => dest.CashDiscount, opt => opt.MapFrom(src => src.CashDiscount))
            // .ForMember(dest => dest.Tax, opt => opt.MapFrom(src => src.Tax))
            // .ForMember(dest => dest.CustomerReceipt, opt => opt.MapFrom(src => src.CustomerReceipt))
            .AfterMap((src, dest) => { dest.PayloadJson = JsonSerializer.Serialize(src); })
            .AfterMap((src, dest) =>
            {
                dest.RawContent =
                    JsonSerializer.Serialize(
                        Utils.SanitizeRequestForLogging(
                            src)); //todo:cadena en bruto ,debe quedar la cadena en bruto en logs fcano 
            });


        // Guarda el raw JSON request.


        CreateMap<CreateTransactionRequestDto, Transactions>()
             .ForMember(dest => dest.TransactionsId, opt => opt.MapFrom(_ => Guid.NewGuid())) // Generar nuevo GUID
            .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId))
            .ForMember(dest => dest.TraceId, opt => opt.MapFrom(src => src.TraceId))
            .ForMember(dest => dest.TypeTransactionsId, opt => opt.MapFrom(src => src.TypeTransactionsId))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow)) // Fecha actual UTC
            //.ForMember(dest => dest.Monto, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.CreatedBy,
                opt => opt.MapFrom(src => src.CreatedBy ?? "system")); // Valor por defecto

        //CreateMap<NmiResponseDto, Transactions>()
           // .ForMember(dest => dest.AuthCode, opt => opt.MapFrom(src => src.AuthCode))
            //.ForMember(dest => dest.orderId, opt => opt.MapFrom(src => src.OrderId))
            // .ForMember(dest => dest.response_code, opt => opt.MapFrom(src =>
            // (TransactionCodeResponse)int.Parse(src.ResponseCode)));

        CreateMap<UpdateTransactionRequestDto, Transactions>()
            .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.ModifiedBy, opt => opt.MapFrom(src => src.ModifiedBy));

        //todo: agregar mapper de tabla de transaccion  roilan
        // Mapear tuple (respuesta simple) -> log de respuesta
        CreateMap<(string response, Guid requestId, string? transactionId, string status, string message),
                NmiTransactionResponseLog>()
            .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.requestId))
            .ForMember(dest => dest.RawResponse, opt => opt.MapFrom(src => src.response))
            .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.transactionId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.message))
            .ForMember(dest => dest.ReceivedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));


        //todo: agregar mapper de tabla de transaccion  roilan
        // Mapear respuesta estructurada DTO -> entidad de log persistente
        CreateMap<NmiResponseDto, TransactionResponse>()
            .ForMember(dest => dest.Response, opt => opt.MapFrom(src => src.Response))
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
            .ForMember(dest => dest.ReceivedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}