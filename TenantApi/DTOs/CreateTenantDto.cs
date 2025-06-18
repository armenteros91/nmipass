using System.ComponentModel.DataAnnotations;

namespace TenantApi.DTOs
{
    public class CreateTenantDto
    {
        [Required]
        public string? Name { get; set; }
    }
}
