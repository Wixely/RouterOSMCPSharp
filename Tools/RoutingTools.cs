using System.ComponentModel;
using ModelContextProtocol.Server;
using RouterOSMCPSharp.Services;

namespace RouterOSMCPSharp.Tools;

[McpServerToolType]
public static class RoutingTools
{
    [McpServerTool(Name = "routing_bgp_session_list"),
     Description("Active BGP sessions and their state.")]
    public static Task<string> BgpSessions(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/routing/bgp/session", ct);
    }

    [McpServerTool(Name = "routing_bgp_peer_list"),
     Description("Configured BGP peers (RouterOS 6 style) - includes their negotiated state on supported builds.")]
    public static Task<string> BgpPeers(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/routing/bgp/peer", ct);
    }

    [McpServerTool(Name = "routing_ospf_neighbors"),
     Description("OSPF neighbors and adjacency state.")]
    public static Task<string> OspfNeighbors(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/routing/ospf/neighbor", ct);
    }

    [McpServerTool(Name = "routing_ospf_instance_list"),
     Description("OSPF instances.")]
    public static Task<string> OspfInstances(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/routing/ospf/instance", ct);
    }

    [McpServerTool(Name = "routing_table_list"),
     Description("Configured routing tables (used for VRF/policy routing).")]
    public static Task<string> RoutingTables(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/routing/table", ct);
    }

    [McpServerTool(Name = "routing_filter_rule_list"),
     Description("Routing filter rules (route-maps).")]
    public static Task<string> FilterRules(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/routing/filter/rule", ct);
    }

    private static void EnsureEnabled(RouterOSService svc)
    {
        if (!svc.Options.EnableRoutingTools)
            throw new InvalidOperationException("Routing tools are disabled (RouterOS:EnableRoutingTools=false).");
    }
}
