using System.ComponentModel;
using ModelContextProtocol.Server;
using RouterOSMCPSharp.Services;

namespace RouterOSMCPSharp.Tools;

[McpServerToolType]
public static class ManagementTools
{
    [McpServerTool(Name = "user_list"),
     Description("Local RouterOS users.")]
    public static Task<string> UserList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/user", ct);
    }

    [McpServerTool(Name = "user_active_list"),
     Description("Currently signed-in RouterOS sessions.")]
    public static Task<string> UserActiveList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/user/active", ct);
    }

    [McpServerTool(Name = "queue_simple_list"),
     Description("Simple queues (per-IP/per-interface bandwidth shapers).")]
    public static Task<string> SimpleQueueList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/queue/simple", ct);
    }

    [McpServerTool(Name = "queue_tree_list"),
     Description("Queue tree entries.")]
    public static Task<string> QueueTreeList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/queue/tree", ct);
    }

    [McpServerTool(Name = "file_list"),
     Description("Files stored on the device (config backups, scripts, certificates).")]
    public static Task<string> FileList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/file", ct);
    }

    [McpServerTool(Name = "certificate_list"),
     Description("Installed certificates.")]
    public static Task<string> CertificateList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/certificate", ct);
    }

    [McpServerTool(Name = "snmp_settings"),
     Description("SNMP daemon settings.")]
    public static Task<string> SnmpSettings(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/snmp", ct);
    }

    private static void EnsureEnabled(RouterOSService svc)
    {
        if (!svc.Options.EnableManagementTools)
            throw new InvalidOperationException("Management tools are disabled (RouterOS:EnableManagementTools=false).");
    }
}
