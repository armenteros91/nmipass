using AutoMapper;
using ThreeTP.Payment.Application.Commands.Terminals;
using ThreeTP.Payment.Application.DTOs.Requests.Terminals;
using ThreeTP.Payment.Application.DTOs.Responses.Terminals;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Application.Mappings
{
    public class TerminalMappingProfile : Profile
    {
        public TerminalMappingProfile()
        {
            // Map from Terminal (Domain Entity) to TerminalResponseDto (Response DTO)
            CreateMap<Terminal, TerminalResponseDto>()
                .ForMember(dest => dest.TerminalId, opt => opt.MapFrom(src => src.TerminalId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate));

            // Map from CreateTerminalRequestDto (Request DTO) to Terminal (Domain Entity)
            CreateMap<CreateTerminalRequestDto, Terminal>()
                .ConstructUsing(src => new Terminal(src.Name, src.TenantId, src.SecretKey))
                // The constructor Terminal(name, tenantId, secretKey) assigns plain secretKey to its SecretKeyEncrypted property.
                // The repository later encrypts the value in SecretKeyEncrypted.
                .ForMember(dest => dest.TerminalId, opt => opt.Ignore()) // TerminalId is generated in Terminal's constructor or by DB
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Handled by BaseEntity or Terminal constructor
                .ForMember(dest => dest.SecretKeyHash, opt => opt.Ignore()); // Handled by repository before persistence

            // If you were mapping from CreateTerminalCommand to Terminal:
            // CreateMap<CreateTerminalCommand, Terminal>()
            //     .ConstructUsing(src => new Terminal(src.TerminalRequest.Name, src.TerminalRequest.TenantId, src.TerminalRequest.SecretKey))
            //     .ForMember(dest => dest.TerminalId, opt => opt.Ignore())
            //     .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            //     .ForMember(dest => dest.SecretKeyHash, opt => opt.Ignore());


            // No specific mapping for UpdateTerminalRequestDto to Terminal here,
            // as the UpdateTerminalCommandHandler and TerminalsController fetch the entity and update fields selectively (manually).
            // If a full map was desired for other uses:
            // CreateMap<UpdateTerminalRequestDto, Terminal>()
            //     .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name != null)) // Apply if Name is provided
            //     .ForMember(dest => dest.IsActive, opt => opt.Condition(src => src.IsActive.HasValue)) // Apply if IsActive is provided
            //     // Be careful with other properties, ensure they are not unintentionally overwritten to null/defaults
            //     .ForAllOtherMembers(opts => opts.Ignore()); // Example to prevent overwriting other fields
        }
    }
}
