using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ThreeTP.Payment.Application.Helpers;
using ThreeTP.Payment.Application.Interfaces;
using ThreeTP.Payment.Domain.Entities.Tenant;

namespace ThreeTP.Payment.Infrastructure.Persistence.Repositories
{
    public class TerminalRepository : GenericRepository<Terminal>, ITerminalRepository
    {
        private readonly ILogger<TerminalRepository> _logger;
        private readonly IEncryptionService _encryptionService;
        private readonly NmiDbContext _dbContext;

        public TerminalRepository(
            ILogger<TerminalRepository> logger,
            IEncryptionService encryptionService,
            NmiDbContext dbContext)
            : base(dbContext, logger)
        {
            _logger = logger;
            _encryptionService = encryptionService;
            _dbContext = dbContext;
        }

        public async Task<Terminal?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting terminal {Id}", id);
            return await GetOneAsync(t => t.TenantId == id);
        }

        public async Task<Terminal?> GetBySecretKeyAsync(string encryptedSecretKey)
        {
            _logger.LogInformation(" Getting SecretKey  with secret key {EncryptedSecretKey}: ", encryptedSecretKey);
            return await GetOneAsync(t => t.SecretKeyEncrypted == encryptedSecretKey);
        }

        public async Task<IEnumerable<Terminal>> GetByTenantIdAsync(Guid tenantId)
        {
            _logger.LogInformation("Getting  tenant for  TenantId : {Tenantid}", tenantId);
            return await GetAllAsync(t => t.TenantId == tenantId);
        }

        public async Task<string?> GetDecryptedSecretKeyAsync(Guid terminalId)
        {
            _logger.LogInformation("Getting decrypted secret key for terminal Id: {TerminalId}", terminalId);
            var terminal = await GetByIdAsync(terminalId);
            return terminal != null
                ? _encryptionService.Decrypt(terminal.SecretKeyEncrypted)
                : null;
        }

        public new async Task AddAsync(Terminal entity)
        {
            _logger.LogInformation("Adding terminal {TerminalEntity} ", JsonConvert.SerializeObject(entity, formatting: Formatting.Indented));
            if (string.IsNullOrWhiteSpace(entity.SecretKeyEncrypted))
                throw new ArgumentException("SecretKeyEncrypted is required.");

            // Encripta y hashea el secreto antes de persistir
            var plainSecretKey = entity.SecretKeyEncrypted;
            entity.SecretKeyEncrypted = _encryptionService.Encrypt(plainSecretKey);
            entity.SecretKeyHash = Utils.ComputeSHA256(plainSecretKey);

            await base.AddAsync(entity);
            _logger.LogInformation("Terminal {TerminalId} added for Tenant {TenantId}", entity.TerminalId, entity.TenantId);
        }

        public async Task UpdateSecretKeyAsync(Guid terminalId, string newPlainSecretKey)
        {
            if (string.IsNullOrWhiteSpace(newPlainSecretKey))
                throw new ArgumentException("Secret key cannot be null or empty.", nameof(newPlainSecretKey));

            _logger.LogInformation("Updating secret key for terminalId ID: {TerminalId}", terminalId);

            var terminal = await GetByIdAsync(terminalId);
            if (terminal == null)
            {
                _logger.LogWarning("Terminal not found for ID: {TerminalId}", terminalId);
                throw new InvalidOperationException("Terminal not found.");
            }

            terminal.SecretKeyEncrypted = _encryptionService.Encrypt(newPlainSecretKey);
            terminal.SecretKeyHash = Utils.ComputeSHA256(newPlainSecretKey);

            _dbContext.Set<Terminal>().Update(terminal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plainSecretName"></param>
        /// <returns></returns>
        public async Task<Terminal?> FindBySecretNameAsync(string plainSecretName)
        {
            if (string.IsNullOrWhiteSpace(plainSecretName))
                throw new ArgumentException("Secret name must not be empty.");
            
            _logger.LogInformation("Getting terminal with secret: {PlainSecret}", plainSecretName);
            
            var hash = Utils.ComputeSHA256(plainSecretName);
            return await _dbContext.Set<Terminal>()
                .FirstOrDefaultAsync(t => t.SecretKeyHash == hash);
        }
        public void Update(Terminal terminal)
        {
            _dbContext.Update(terminal); // track for save entity
        }
    }
}