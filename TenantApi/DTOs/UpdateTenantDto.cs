using System.ComponentModel.DataAnnotations;

namespace TenantApi.DTOs
{
    public class UpdateTenantDto
    {
        [Required]
        public string? Name { get; set; }
    }
}
