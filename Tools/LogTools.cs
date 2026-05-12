using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using RouterOSMCPSharp.Services;

namespace RouterOSMCPSharp.Tools;

[McpServerToolType]
public static class LogTools
{
    [McpServerTool(Name = "log_print"),
     Description("Recent entries from the RouterOS log buffer. Capped at RouterOS:MaxLogEntries.")]
    public static async Task<string> Print(
        RouterOSService svc,
        [Description("Optional topic substring to grep for (e.g. 'firewall', 'wireless').")] string? topic = null,
        [Description("Maximum entries to return. Defaults to RouterOS:MaxLogEntries.")] int? limit = null,
        CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        var max = Math.Min(limit ?? svc.Options.MaxLogEntries, svc.Options.MaxLogEntries);
        if (svc.Rest.Enabled)
        {
            var path = string.IsNullOrWhiteSpace(topic)
                ? "/log"
                : $"/log?topics~{Uri.EscapeDataString(topic)}";
            var json = await svc.Rest.GetAsync(path, ct);
            // Trim to max entries on the client side - RouterOS doesn't accept .proplist/limit on /log via REST.
            if (json.ValueKind == JsonValueKind.Array)
            {
                var trimmed = json.EnumerateArray().TakeLast(max).ToArray();
                return JsonSerializer.Serialize(trimmed, JsonOpts.Default);
            }
            return json.GetRawText();
        }
        var filter = string.IsNullOrWhiteSpace(topic) ? string.Empty : $" where topics~\"{topic}\"";
        var result = await svc.Ssh.ExecuteAsync($"/log print{filter}", ct);
        var lines = result.Output.Split('\n').TakeLast(max);
        return JsonSerializer.Serialize(new { stdout = string.Join('\n', lines), stderr = result.Error, exitStatus = result.ExitStatus }, JsonOpts.Default);
    }

    private static void EnsureEnabled(RouterOSService svc)
    {
        if (!svc.Options.EnableLogTools)
            throw new InvalidOperationException("Log tools are disabled (RouterOS:EnableLogTools=false).");
    }
}
