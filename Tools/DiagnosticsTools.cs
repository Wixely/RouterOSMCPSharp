using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using RouterOSMCPSharp.Services;

namespace RouterOSMCPSharp.Tools;

[McpServerToolType]
public static class DiagnosticsTools
{
    [McpServerTool(Name = "diag_ping"),
     Description("Run a bounded ping from the router to a target. Returns aggregated stats.")]
    public static async Task<string> Ping(
        RouterOSService svc,
        [Description("Target host or IP.")] string target,
        [Description("Number of echo requests to send (default 5).")] int count = 5,
        [Description("Optional source interface or address.")] string? source = null,
        CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        if (svc.Rest.Enabled)
        {
            var body = source is null
                ? (object)new { address = target, count }
                : new { address = target, count, src = source };
            var json = await svc.Rest.PostAsync("/ping", body, ct);
            return json.GetRawText();
        }
        var srcArg = string.IsNullOrWhiteSpace(source) ? string.Empty : $" src-address={source}";
        var result = await svc.Ssh.ExecuteAsync($"/ping {target} count={count}{srcArg}", ct);
        return JsonSerializer.Serialize(new { stdout = result.Output, stderr = result.Error, exitStatus = result.ExitStatus }, JsonOpts.Default);
    }

    [McpServerTool(Name = "diag_traceroute"),
     Description("Run a traceroute from the router to a target.")]
    public static async Task<string> Traceroute(
        RouterOSService svc,
        [Description("Target host or IP.")] string target,
        [Description("Maximum number of probes (default 10).")] int count = 10,
        CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        if (svc.Rest.Enabled)
        {
            var json = await svc.Rest.PostAsync("/tool/traceroute", new { address = target, count }, ct);
            return json.GetRawText();
        }
        var result = await svc.Ssh.ExecuteAsync($"/tool traceroute {target} count={count}", ct);
        return JsonSerializer.Serialize(new { stdout = result.Output, stderr = result.Error, exitStatus = result.ExitStatus }, JsonOpts.Default);
    }

    [McpServerTool(Name = "diag_resolve_dns"),
     Description("Resolve a DNS name using the router's resolver.")]
    public static async Task<string> ResolveDns(
        RouterOSService svc,
        [Description("Name to resolve.")] string name,
        CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        if (svc.Rest.Enabled)
        {
            var json = await svc.Rest.PostAsync("/resolve", new { name }, ct);
            return json.GetRawText();
        }
        var result = await svc.Ssh.ExecuteAsync($":put [:resolve {name}]", ct);
        return JsonSerializer.Serialize(new { stdout = result.Output, stderr = result.Error, exitStatus = result.ExitStatus }, JsonOpts.Default);
    }

    [McpServerTool(Name = "diag_torch_snapshot"),
     Description("Capture a short Torch snapshot on an interface (per-flow live traffic). Bounded by SshCommandTimeoutSeconds.")]
    public static async Task<string> TorchSnapshot(
        RouterOSService svc,
        [Description("Interface name to torch (e.g. ether1).")] string name,
        [Description("Snapshot duration in seconds (default 5).")] int durationSeconds = 5,
        CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        var result = await svc.Ssh.ExecuteAsync($"/tool torch interface={name} duration={durationSeconds}", ct);
        return JsonSerializer.Serialize(new { stdout = result.Output, stderr = result.Error, exitStatus = result.ExitStatus }, JsonOpts.Default);
    }

    private static void EnsureEnabled(RouterOSService svc)
    {
        if (!svc.Options.EnableDiagnosticsTools)
            throw new InvalidOperationException("Diagnostics tools are disabled (RouterOS:EnableDiagnosticsTools=false).");
    }
}
