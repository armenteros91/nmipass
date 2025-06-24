namespace ThreeTP.Payment.Application.Interfaces.aws;

public interface IAwsSecretSyncService
{
    Task SyncSecretToTerminalAsync(Guid terminalId, string secretString, CancellationToken cancellationToken);
}