using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using RouterOSMCPSharp.Services;

namespace RouterOSMCPSharp.Tools;

[McpServerToolType]
public static class CommandTools
{
    [McpServerTool(Name = "run_command"),
     Description("Execute an arbitrary RouterOS CLI command via SSH. " +
                 "Gated by RouterOS:AllowArbitraryCommands and RouterOS:CommandDenyList. " +
                 "Disabled by default; in read-only mode requires AllowArbitraryCommandsInReadOnly=true.")]
    public static async Task<string> RunCommand(
        RouterOSService svc,
        [Description("Full RouterOS CLI command, e.g. '/system identity print' or ':put [/system resource get cpu-load]'.")]
        string command,
        CancellationToken ct = default)
    {
        svc.EnsureArbitraryCommandsAllowed(command);
        var result = await svc.Ssh.ExecuteAsync(command, ct);
        return JsonSerializer.Serialize(new
        {
            command,
            stdout = result.Output,
            stderr = result.Error,
            exitStatus = result.ExitStatus,
            success = result.Success,
        }, JsonOpts.Default);
    }

    [McpServerTool(Name = "run_rest"),
     Description("Issue a raw REST call against the RouterOS v7+ REST API. " +
                 "GET requests are always allowed; POST/PATCH/DELETE require write mode.")]
    public static async Task<string> RunRest(
        RouterOSService svc,
        [Description("HTTP method: GET, POST, PATCH, DELETE.")] string method,
        [Description("REST resource path, e.g. '/system/resource' or '/interface/ethernet'.")] string path,
        [Description("Optional JSON body for POST/PATCH (string).")] string? jsonBody = null,
        CancellationToken ct = default)
    {
        if (!svc.Rest.Enabled)
            throw new InvalidOperationException("REST API is disabled (RouterOS:EnableRestApi=false).");

        var verb = method.Trim().ToUpperInvariant();
        if (verb is "POST" or "PATCH" or "DELETE")
        {
            svc.EnsureWriteAllowed($"run_rest {verb}");
        }
        else if (verb != "GET")
        {
            throw new ArgumentException($"Unsupported method '{method}'. Use GET, POST, PATCH or DELETE.");
        }

        object? body = null;
        if (!string.IsNullOrWhiteSpace(jsonBody))
        {
            body = JsonSerializer.Deserialize<JsonElement>(jsonBody);
        }

        var result = verb switch
        {
            "GET" => await svc.Rest.GetAsync(path, ct),
            "POST" => await svc.Rest.PostAsync(path, body, ct),
            "PATCH" => await svc.Rest.PatchAsync(path, body!, ct),
            "DELETE" => await svc.Rest.DeleteAsync(path, ct),
            _ => throw new ArgumentException(method),
        };
        return result.GetRawText();
    }
}
