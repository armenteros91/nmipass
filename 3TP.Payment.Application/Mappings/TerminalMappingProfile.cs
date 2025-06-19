using AutoMapper;
using ThreeTP.Payment.Application.Commands.Terminals; // For CreateTerminalCommand if mapping from it
using ThreeTP.Payment.Application.DTOs.Requests.Terminals; // For CreateTerminalRequestDto
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
            // This is useful if you want to map directly in the service or before sending to handler,
            // though the current CreateTerminalCommandHandler does manual mapping.
            // The SecretKey is mapped directly; encryption is handled by the repository.
            CreateMap<CreateTerminalRequestDto, Terminal>()
                .ConstructUsing(src => new Terminal(src.Name, src.TenantId, src.SecretKey))
                .ForMember(dest => dest.TerminalId, opt => opt.Ignore()) // TerminalId is generated in Terminal constructor
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Handled by BaseEntity or Terminal constructor
                .ForMember(dest => dest.SecretKeyEncrypted, opt => opt.MapFrom(src => src.SecretKey)) // Map plain key, repo encrypts
                .ForMember(dest => dest.SecretKeyHash, opt => opt.Ignore()); // Handled by repository

            // If you were mapping from CreateTerminalCommand to Terminal:
            // CreateMap<CreateTerminalCommand, Terminal>()
            //     .ConstructUsing(src => new Terminal(src.TerminalRequest.Name, src.TerminalRequest.TenantId, src.TerminalRequest.SecretKey))
            //     .ForMember(dest => dest.TerminalId, opt => opt.Ignore())
            //     .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            //     .ForMember(dest => dest.SecretKeyEncrypted, opt => opt.MapFrom(src => src.TerminalRequest.SecretKey))
            //     .ForMember(dest => dest.SecretKeyHash, opt => opt.Ignore());


            // No specific mapping for UpdateTerminalRequestDto to Terminal here,
            // as the UpdateTerminalCommandHandler fetches the entity and updates fields selectively.
            // If a full map was desired:
            // CreateMap<UpdateTerminalRequestDto, Terminal>()
            //     .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name != null)) // Apply if Name is provided
            //     .ForMember(dest => dest.IsActive, opt => opt.Condition(src => src.IsActive.HasValue)) // Apply if IsActive is provided
            //     // Be careful with other properties, ensure they are not unintentionally overwritten to null/defaults
            //     .ForAllOtherMembers(opts => opts.Ignore()); // Example to prevent overwriting other fields
        }
    }
}
