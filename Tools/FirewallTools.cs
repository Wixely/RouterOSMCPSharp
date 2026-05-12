using System.ComponentModel;
using ModelContextProtocol.Server;
using RouterOSMCPSharp.Services;

namespace RouterOSMCPSharp.Tools;

[McpServerToolType]
public static class FirewallTools
{
    [McpServerTool(Name = "firewall_filter_list"),
     Description("IPv4 firewall filter rules (input/forward/output chains).")]
    public static Task<string> FilterList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/firewall/filter", ct);
    }

    [McpServerTool(Name = "firewall_nat_list"),
     Description("IPv4 firewall NAT rules.")]
    public static Task<string> NatList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/firewall/nat", ct);
    }

    [McpServerTool(Name = "firewall_mangle_list"),
     Description("IPv4 firewall mangle rules (marking, MTU clamping, etc.).")]
    public static Task<string> MangleList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/firewall/mangle", ct);
    }

    [McpServerTool(Name = "firewall_raw_list"),
     Description("IPv4 firewall raw table entries.")]
    public static Task<string> RawList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/firewall/raw", ct);
    }

    [McpServerTool(Name = "firewall_address_list"),
     Description("IPv4 firewall address-list entries.")]
    public static Task<string> AddressListList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/firewall/address-list", ct);
    }

    [McpServerTool(Name = "firewall_connections"),
     Description("Connection tracking table (active connections).")]
    public static Task<string> Connections(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/firewall/connection", ct);
    }

    [McpServerTool(Name = "firewall_ipv6_filter_list"),
     Description("IPv6 firewall filter rules.")]
    public static Task<string> Ipv6FilterList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ipv6/firewall/filter", ct);
    }

    [McpServerTool(Name = "firewall_enable_rule"),
     Description("Enable a firewall filter rule by id (e.g. '*1A'). Disabled in read-only mode.")]
    public static async Task<string> EnableRule(
        RouterOSService svc,
        [Description("Rule id, e.g. *1A.")] string id,
        CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        svc.EnsureWriteAllowed("firewall_enable_rule");
        if (svc.Rest.Enabled)
        {
            var json = await svc.Rest.PostAsync("/ip/firewall/filter/enable", new { numbers = id }, ct);
            return json.GetRawText();
        }
        var result = await svc.Ssh.ExecuteAsync($"/ip firewall filter enable {id}", ct);
        return System.Text.Json.JsonSerializer.Serialize(new { stdout = result.Output, stderr = result.Error, exitStatus = result.ExitStatus }, JsonOpts.Default);
    }

    [McpServerTool(Name = "firewall_disable_rule"),
     Description("Disable a firewall filter rule by id (e.g. '*1A'). Disabled in read-only mode.")]
    public static async Task<string> DisableRule(
        RouterOSService svc,
        [Description("Rule id, e.g. *1A.")] string id,
        CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        svc.EnsureWriteAllowed("firewall_disable_rule");
        if (svc.Rest.Enabled)
        {
            var json = await svc.Rest.PostAsync("/ip/firewall/filter/disable", new { numbers = id }, ct);
            return json.GetRawText();
        }
        var result = await svc.Ssh.ExecuteAsync($"/ip firewall filter disable {id}", ct);
        return System.Text.Json.JsonSerializer.Serialize(new { stdout = result.Output, stderr = result.Error, exitStatus = result.ExitStatus }, JsonOpts.Default);
    }

    private static void EnsureEnabled(RouterOSService svc)
    {
        if (!svc.Options.EnableFirewallTools)
            throw new InvalidOperationException("Firewall tools are disabled (RouterOS:EnableFirewallTools=false).");
    }
}
