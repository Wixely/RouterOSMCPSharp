using Microsoft.Extensions.Options;
using RouterOSMCPSharp.Configuration;

namespace RouterOSMCPSharp.Services;

/// <summary>
/// Facade that exposes both the SSH and REST clients to MCP tools and centralises
/// the read-only / arbitrary-command policy checks.
/// </summary>
public sealed class RouterOSService
{
    private readonly RouterOSOptions _options;

    public RouterOSService(IOptions<RouterOSOptions> options, RouterOSSshClient ssh, RouterOSRestClient rest)
    {
        _options = options.Value;
        Ssh = ssh;
        Rest = rest;
    }

    public RouterOSOptions Options => _options;
    public RouterOSSshClient Ssh { get; }
    public RouterOSRestClient Rest { get; }

    public bool IsReadOnly => _options.ReadOnly;

    public void EnsureWriteAllowed(string operation)
    {
        if (_options.ReadOnly)
        {
            throw new InvalidOperationException(
                $"Operation '{operation}' is blocked: server is running in read-only mode. " +
                "Set RouterOS:ReadOnly=false to allow writes.");
        }
    }

    public void EnsureArbitraryCommandsAllowed(string command)
    {
        if (!_options.AllowArbitraryCommands)
        {
            throw new InvalidOperationException(
                "Arbitrary command execution is disabled. Set RouterOS:AllowArbitraryCommands=true to enable.");
        }
        if (_options.ReadOnly && !_options.AllowArbitraryCommandsInReadOnly)
        {
            throw new InvalidOperationException(
                "Arbitrary commands are gated behind read-only mode. " +
                "Set RouterOS:ReadOnly=false, or RouterOS:AllowArbitraryCommandsInReadOnly=true to override.");
        }
        foreach (var blocked in _options.CommandDenyList)
        {
            if (!string.IsNullOrWhiteSpace(blocked) &&
                command.Contains(blocked, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Command rejected: contains deny-listed substring '{blocked}'.");
            }
        }
    }
}
