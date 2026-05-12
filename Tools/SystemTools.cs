using System.ComponentModel;
using ModelContextProtocol.Server;
using RouterOSMCPSharp.Services;

namespace RouterOSMCPSharp.Tools;

[McpServerToolType]
public static class SystemTools
{
    [McpServerTool(Name = "system_identity"),
     Description("Get the device identity (name).")]
    public static Task<string> GetIdentity(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/system/identity", ct);
    }

    [McpServerTool(Name = "system_resource"),
     Description("CPU load, free/total memory, uptime, board name, RouterOS version and architecture.")]
    public static Task<string> GetResource(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/system/resource", ct);
    }

    [McpServerTool(Name = "system_routerboard"),
     Description("RouterBOARD info: model, serial number, current and upgrade firmware versions.")]
    public static Task<string> GetRouterBoard(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/system/routerboard", ct);
    }

    [McpServerTool(Name = "system_health"),
     Description("Hardware health metrics (temperatures, voltages, fan speeds) where the device exposes them.")]
    public static Task<string> GetHealth(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/system/health", ct);
    }

    [McpServerTool(Name = "system_clock"),
     Description("Current time, date, timezone and DST settings.")]
    public static Task<string> GetClock(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/system/clock", ct);
    }

    [McpServerTool(Name = "system_license"),
     Description("Software ID and license details.")]
    public static Task<string> GetLicense(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/system/license", ct);
    }

    [McpServerTool(Name = "system_package_list"),
     Description("Installed RouterOS packages and their versions.")]
    public static Task<string> ListPackages(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/system/package", ct);
    }

    [McpServerTool(Name = "system_history"),
     Description("Recent configuration change history.")]
    public static Task<string> GetHistory(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/system/history", ct);
    }

    [McpServerTool(Name = "system_reboot"),
     Description("Reboot the router. Disabled in read-only mode.")]
    public static Task<string> Reboot(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        svc.EnsureWriteAllowed("system_reboot");
        return ToolHelpers.ActionAsync(svc, "/system/reboot", null, "/system reboot", ct);
    }

    private static void EnsureEnabled(RouterOSService svc)
    {
        if (!svc.Options.EnableSystemTools)
            throw new InvalidOperationException("System tools are disabled (RouterOS:EnableSystemTools=false).");
    }
}
