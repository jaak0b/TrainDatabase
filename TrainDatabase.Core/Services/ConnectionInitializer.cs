using System.Net;
using Microsoft.Extensions.Logging;
using TrainDatabase.Core.Ports;

namespace TrainDatabase.Core.Services;

public sealed class ConnectionInitializer(IClientAdapter client, ISettingsStore settings, ILogger<ConnectionInitializer> logger) : IConnectionInitializer
{
    private const string ClientIpKey = "ClientIP";
    private const int Z21Port = 21105;

    public Task ConnectAsync()
    {
        string? configured = settings.Get(ClientIpKey);
        if (string.IsNullOrWhiteSpace(configured))
        {
            return Task.CompletedTask;
        }

        if (TryResolveEndPoint(configured.Trim(), out IPEndPoint endPoint))
        {
            client.Connect(endPoint);
        }
        else
        {
            logger.LogWarning("Configured command station address '{Address}' is not a valid IP address; skipping connection.", configured);
        }

        return Task.CompletedTask;
    }

    private static bool TryResolveEndPoint(string configured, out IPEndPoint endPoint)
    {
        if (IPEndPoint.TryParse(configured, out IPEndPoint? parsed) && parsed is not null)
        {
            endPoint = parsed.Port == 0 ? new IPEndPoint(parsed.Address, Z21Port) : parsed;
            return true;
        }

        endPoint = new IPEndPoint(IPAddress.None, 0);
        return false;
    }
}
