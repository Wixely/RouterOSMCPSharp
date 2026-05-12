using System.Text.Json;
using RouterOSMCPSharp.Services;

namespace RouterOSMCPSharp.Tools;

internal static class ToolHelpers
{
    /// <summary>
    /// Run a print-style query. Prefers the structured REST API; falls back to SSH `<path> print`
    /// when REST is disabled. The path is the RouterOS resource path (e.g. "/interface/ethernet").
    /// </summary>
    public static async Task<string> PrintAsync(RouterOSService svc, string path, CancellationToken ct = default)
    {
        if (svc.Rest.Enabled)
        {
            var json = await svc.Rest.GetAsync(path, ct).ConfigureAwait(false);
            return json.GetRawText();
        }
        var cli = NormaliseToCli(path) + " print";
        var result = await svc.Ssh.ExecuteAsync(cli, ct).ConfigureAwait(false);
        return JsonSerializer.Serialize(new { stdout = result.Output, stderr = result.Error, exitStatus = result.ExitStatus }, JsonOpts.Default);
    }

    /// <summary>
    /// Run an action endpoint (POST) such as /system/reboot or /ping. Falls back to a one-shot SSH command.
    /// </summary>
    public static async Task<string> ActionAsync(RouterOSService svc, string path, object? body, string sshFallbackCommand, CancellationToken ct = default)
    {
        if (svc.Rest.Enabled)
        {
            var json = await svc.Rest.PostAsync(path, body, ct).ConfigureAwait(false);
            return json.GetRawText();
        }
        var result = await svc.Ssh.ExecuteAsync(sshFallbackCommand, ct).ConfigureAwait(false);
        return JsonSerializer.Serialize(new { stdout = result.Output, stderr = result.Error, exitStatus = result.ExitStatus }, JsonOpts.Default);
    }

    /// <summary>Convert a REST path "/interface/ethernet" to the equivalent CLI "/interface ethernet".</summary>
    public static string NormaliseToCli(string restPath)
    {
        var trimmed = restPath.Trim('/');
        if (trimmed.Length == 0) return "/";
        var firstSlash = trimmed.IndexOf('/');
        if (firstSlash < 0) return "/" + trimmed;
        return "/" + trimmed[..firstSlash] + " " + trimmed[(firstSlash + 1)..].Replace('/', ' ');
    }
}
