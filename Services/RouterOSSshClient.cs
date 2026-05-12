using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Common;
using RouterOSMCPSharp.Configuration;

namespace RouterOSMCPSharp.Services;

/// <summary>
/// Thin wrapper around SSH.NET that opens a fresh SSH session per command. RouterOS sessions are cheap
/// and a per-call connection avoids state leaking between concurrent MCP tool invocations.
/// </summary>
public sealed class RouterOSSshClient
{
    private readonly RouterOSOptions _options;
    private readonly ILogger<RouterOSSshClient> _logger;

    public RouterOSSshClient(IOptions<RouterOSOptions> options, ILogger<RouterOSSshClient> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SshCommandResult> ExecuteAsync(string command, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        // RouterOS SSH:
        //   "+ct"  - disable colour and terminal-detect so output is plain text suitable for parsing
        //   "+800w" - set 800-column wrap so columns are not truncated
        // The username is suffixed: "admin+ct800w".
        var user = $"{_options.Username}+ct800w";

        using var client = CreateClient(user);
        client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(_options.SshTimeoutSeconds);

        try
        {
            await Task.Run(() => client.Connect(), ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect SSH session to {Host}:{Port}", _options.Host, _options.SshPort);
            throw;
        }

        try
        {
            using var cmd = client.CreateCommand(command);
            cmd.CommandTimeout = TimeSpan.FromSeconds(_options.SshCommandTimeoutSeconds);

            var execTask = Task.Factory.FromAsync(cmd.BeginExecute(), cmd.EndExecute);
            await using (ct.Register(() => { try { client.Disconnect(); } catch { } }))
            {
                await execTask.ConfigureAwait(false);
            }

            return new SshCommandResult(cmd.Result ?? string.Empty, cmd.Error ?? string.Empty, cmd.ExitStatus ?? 0);
        }
        finally
        {
            try { client.Disconnect(); } catch { /* best effort */ }
        }
    }

    private SshClient CreateClient(string user)
    {
        AuthenticationMethod auth;
        if (!string.IsNullOrWhiteSpace(_options.PrivateKeyPath))
        {
            var keyFile = string.IsNullOrEmpty(_options.PrivateKeyPassphrase)
                ? new PrivateKeyFile(_options.PrivateKeyPath!)
                : new PrivateKeyFile(_options.PrivateKeyPath!, _options.PrivateKeyPassphrase);
            auth = new PrivateKeyAuthenticationMethod(user, keyFile);
        }
        else
        {
            auth = new PasswordAuthenticationMethod(user, _options.Password ?? string.Empty);
        }

        var connectionInfo = new ConnectionInfo(_options.Host, _options.SshPort, user, auth);
        var client = new SshClient(connectionInfo);

        if (!string.IsNullOrWhiteSpace(_options.ExpectedHostKeyFingerprint))
        {
            var expected = _options.ExpectedHostKeyFingerprint!.Trim();
            client.HostKeyReceived += (_, e) =>
            {
                var actual = Convert.ToBase64String(SHA256.HashData(e.HostKey));
                if (!string.Equals(expected, actual, StringComparison.Ordinal) &&
                    !string.Equals(expected, "SHA256:" + actual, StringComparison.Ordinal))
                {
                    _logger.LogError("SSH host key mismatch for {Host}: expected {Expected}, got SHA256:{Actual}",
                        _options.Host, expected, actual);
                    e.CanTrust = false;
                }
            };
        }
        return client;
    }
}

public sealed record SshCommandResult(string Output, string Error, int ExitStatus)
{
    public bool Success => ExitStatus == 0 && string.IsNullOrEmpty(Error);
}
