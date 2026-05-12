using System.ComponentModel;
using ModelContextProtocol.Server;
using RouterOSMCPSharp.Services;

namespace RouterOSMCPSharp.Tools;

[McpServerToolType]
public static class IpTools
{
    [McpServerTool(Name = "ip_address_list"),
     Description("List IPv4 addresses configured on the device.")]
    public static Task<string> AddressList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/address", ct);
    }

    [McpServerTool(Name = "ip_route_list"),
     Description("Active IPv4 routing table including dynamic routes.")]
    public static Task<string> RouteList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/route", ct);
    }

    [McpServerTool(Name = "ip_arp_list"),
     Description("ARP table entries.")]
    public static Task<string> ArpList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/arp", ct);
    }

    [McpServerTool(Name = "ip_neighbor_list"),
     Description("Neighbor discovery table (CDP/MNDP/LLDP).")]
    public static Task<string> NeighborList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/neighbor", ct);
    }

    [McpServerTool(Name = "ip_dns_settings"),
     Description("DNS resolver configuration.")]
    public static Task<string> DnsSettings(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/dns", ct);
    }

    [McpServerTool(Name = "ip_dns_cache"),
     Description("DNS resolver cache contents.")]
    public static Task<string> DnsCache(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/dns/cache", ct);
    }

    [McpServerTool(Name = "ip_dhcp_server_list"),
     Description("DHCP server instances.")]
    public static Task<string> DhcpServerList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/dhcp-server", ct);
    }

    [McpServerTool(Name = "ip_dhcp_lease_list"),
     Description("Active and bound DHCP leases.")]
    public static Task<string> DhcpLeaseList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/dhcp-server/lease", ct);
    }

    [McpServerTool(Name = "ip_dhcp_client_list"),
     Description("DHCP client status (e.g. WAN client lease info).")]
    public static Task<string> DhcpClientList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/dhcp-client", ct);
    }

    [McpServerTool(Name = "ip_pool_list"),
     Description("IP pools (used by DHCP/PPP).")]
    public static Task<string> PoolList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/pool", ct);
    }

    [McpServerTool(Name = "ip_service_list"),
     Description("Management service ports (api, api-ssl, ssh, www, www-ssl, winbox, ftp, telnet) with allow-from rules.")]
    public static Task<string> ServiceList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ip/service", ct);
    }

    [McpServerTool(Name = "ipv6_address_list"),
     Description("List IPv6 addresses configured on the device.")]
    public static Task<string> Ipv6AddressList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ipv6/address", ct);
    }

    [McpServerTool(Name = "ipv6_route_list"),
     Description("Active IPv6 routing table.")]
    public static Task<string> Ipv6RouteList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/ipv6/route", ct);
    }

    private static void EnsureEnabled(RouterOSService svc)
    {
        if (!svc.Options.EnableIpTools)
            throw new InvalidOperationException("IP tools are disabled (RouterOS:EnableIpTools=false).");
    }
}
