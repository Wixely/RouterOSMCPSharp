using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using RouterOSMCPSharp.Services;

namespace RouterOSMCPSharp.Tools;

[McpServerToolType]
public static class InterfaceTools
{
    [McpServerTool(Name = "interface_list"),
     Description("List all interfaces on the device with type, MTU, MAC, running and disabled flags, and tx/rx byte counters.")]
    public static Task<string> List(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/interface", ct);
    }

    [McpServerTool(Name = "interface_ethernet_list"),
     Description("List ethernet interfaces and their advertised speed/duplex/auto-negotiation state.")]
    public static Task<string> EthernetList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/interface/ethernet", ct);
    }

    [McpServerTool(Name = "interface_vlan_list"),
     Description("List VLAN interfaces.")]
    public static Task<string> VlanList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/interface/vlan", ct);
    }

    [McpServerTool(Name = "interface_bridge_list"),
     Description("List bridges.")]
    public static Task<string> BridgeList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/interface/bridge", ct);
    }

    [McpServerTool(Name = "interface_bridge_port_list"),
     Description("List bridge port memberships.")]
    public static Task<string> BridgePortList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/interface/bridge/port", ct);
    }

    [McpServerTool(Name = "interface_bonding_list"),
     Description("List bonding interfaces.")]
    public static Task<string> BondingList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/interface/bonding", ct);
    }

    [McpServerTool(Name = "interface_monitor_traffic"),
     Description("One-shot traffic snapshot for a single interface (rx/tx bits per second). Equivalent to '/interface monitor-traffic <name> once'.")]
    public static async Task<string> MonitorTraffic(
        RouterOSService svc,
        [Description("Interface name, e.g. 'ether1' or 'wlan1'.")] string name,
        CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        if (svc.Rest.Enabled)
        {
            var json = await svc.Rest.PostAsync("/interface/monitor-traffic", new { numbers = name, once = "" }, ct);
            return json.GetRawText();
        }
        var result = await svc.Ssh.ExecuteAsync($"/interface monitor-traffic {name} once", ct);
        return JsonSerializer.Serialize(new { stdout = result.Output, stderr = result.Error, exitStatus = result.ExitStatus }, JsonOpts.Default);
    }

    [McpServerTool(Name = "interface_print_stats"),
     Description("Detailed RX/TX counters and error statistics for all interfaces.")]
    public static async Task<string> PrintStats(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        var result = await svc.Ssh.ExecuteAsync("/interface print stats-detail", ct);
        return JsonSerializer.Serialize(new { stdout = result.Output, stderr = result.Error, exitStatus = result.ExitStatus }, JsonOpts.Default);
    }

    [McpServerTool(Name = "interface_enable"),
     Description("Enable an interface by name. Disabled in read-only mode.")]
    public static async Task<string> Enable(
        RouterOSService svc,
        [Description("Interface name to enable.")] string name,
        CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        svc.EnsureWriteAllowed("interface_enable");
        if (svc.Rest.Enabled)
        {
            var json = await svc.Rest.PostAsync("/interface/enable", new { numbers = name }, ct);
            return json.GetRawText();
        }
        var result = await svc.Ssh.ExecuteAsync($"/interface enable {name}", ct);
        return JsonSerializer.Serialize(new { stdout = result.Output, stderr = result.Error, exitStatus = result.ExitStatus }, JsonOpts.Default);
    }

    [McpServerTool(Name = "interface_disable"),
     Description("Disable an interface by name. Disabled in read-only mode.")]
    public static async Task<string> Disable(
        RouterOSService svc,
        [Description("Interface name to disable.")] string name,
        CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        svc.EnsureWriteAllowed("interface_disable");
        if (svc.Rest.Enabled)
        {
            var json = await svc.Rest.PostAsync("/interface/disable", new { numbers = name }, ct);
            return json.GetRawText();
        }
        var result = await svc.Ssh.ExecuteAsync($"/interface disable {name}", ct);
        return JsonSerializer.Serialize(new { stdout = result.Output, stderr = result.Error, exitStatus = result.ExitStatus }, JsonOpts.Default);
    }

    private static void EnsureEnabled(RouterOSService svc)
    {
        if (!svc.Options.EnableInterfaceTools)
            throw new InvalidOperationException("Interface tools are disabled (RouterOS:EnableInterfaceTools=false).");
    }
}
