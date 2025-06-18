using System;

namespace TenantApi.Models
{
    public class Tenant
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
