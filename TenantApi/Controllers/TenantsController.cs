using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TenantApi.Models;
using TenantApi.DTOs;

namespace TenantApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private static readonly List<Tenant> _tenants = new List<Tenant>();

        // POST api/tenants
        [HttpPost]
        public IActionResult CreateTenant([FromBody] CreateTenantDto createTenantDto)
        {
            if (createTenantDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = createTenantDto.Name,
                CreatedAt = DateTime.UtcNow
            };
            _tenants.Add(tenant);
            // For POST, typically return 201 Created with a link to the new resource and the resource itself
            return CreatedAtAction(nameof(GetTenantById), new { id = tenant.Id }, tenant);
        }

        // PUT api/tenants/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateTenant(Guid id, [FromBody] UpdateTenantDto updateTenantDto)
        {
            if (updateTenantDto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tenant = _tenants.FirstOrDefault(t => t.Id == id);
            if (tenant == null)
            {
                return NotFound();
            }

            tenant.Name = updateTenantDto.Name;
            return NoContent(); // Or return Ok(tenant) if you want to return the updated object
        }

        // DELETE api/tenants/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteTenant(Guid id)
        {
            var tenant = _tenants.FirstOrDefault(t => t.Id == id);
            if (tenant == null)
            {
                return NotFound();
            }

            _tenants.Remove(tenant);
            return NoContent();
        }

        // GET api/tenants
        [HttpGet]
        public IActionResult GetAllTenants()
        {
            return Ok(_tenants);
        }

        // GET api/tenants/{id}
        [HttpGet("{id}", Name = "GetTenantById")] // Ensure Name matches CreatedAtAction
        public IActionResult GetTenantById(Guid id)
        {
            var tenant = _tenants.FirstOrDefault(t => t.Id == id);
            if (tenant == null)
            {
                return NotFound(); // Standard response for resource not found
            }
            return Ok(tenant);
        }
    }
}
