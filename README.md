# RouterOSMCPSharp

A Model Context Protocol (MCP) server for Mikrotik **RouterOS**. Exposes a broad surface
of RouterOS features so an MCP client can read statistics, diagnose issues, and
(when permitted) make changes.

## Highlights

- **Two transports to the device** — first-class RouterOS v7+ REST API (HTTPS) for
  structured JSON, with SSH as a fallback and the channel for arbitrary commands.
- **Read-only by default** — every write/modify/delete tool is gated behind
  `RouterOS:ReadOnly=false`.
- **Arbitrary commands are double-gated** — `RouterOS:AllowArbitraryCommands=true` plus
  either non-read-only mode or `AllowArbitraryCommandsInReadOnly=true`, with a
  configurable substring deny-list.
- **Runs anywhere** — Docker, Windows Service, or plain console.

## Quick start

### Docker (recommended)

```bash
docker run --rm -it \
  -p 5707:5707 \
  -v $(pwd)/logs:/app/logs \
  -e ROUTEROSMCP_RouterOS__Host=192.168.88.1 \
  -e ROUTEROSMCP_RouterOS__Username=admin \
  -e ROUTEROSMCP_RouterOS__Password=changeme \
  -e ROUTEROSMCP_Server__Password=change-me \
  ghcr.io/wixely/routerosmcpsharp:latest
```

The MCP HTTP endpoint is then available at `http://localhost:5707/mcp`.

### Standalone

```bash
dotnet run --project RouterOSMCPSharp.csproj
```

Edit [RouterOSMCPSharp.json](RouterOSMCPSharp.json) (or use environment variables) to point
at your router. Local-only overrides go in `RouterOSMCPSharp.Local.json`, which is
gitignored.

### Windows Service

Publish the framework-dependent or self-contained build, then register it:

```powershell
sc.exe create RouterOSMCPSharp binPath= "C:\Path\To\RouterOSMCPSharp.exe" start= auto
sc.exe start RouterOSMCPSharp
```

When started under SCM the host detects this and routes logs/config relative to the
executable directory.

## Configuration

All settings live under the `RouterOS` and `Server` sections of `RouterOSMCPSharp.json`.
Override any field with environment variables using the `ROUTEROSMCP_` prefix and the standard double-underscore
convention, e.g. `ROUTEROSMCP_RouterOS__Password`, `ROUTEROSMCP_Server__Port`.
Arrays use numeric indexes, for example `ROUTEROSMCP_RouterOS__CommandDenyList__0=reset`. Booleans use `true` or `false`.

`Server:Password` is blank by default. Set it to require an MCP endpoint password; clients may send `Authorization: Bearer <password>`, the Basic auth password, or `X-MCP-Password`.

Notable knobs:

| Key | Default | Purpose |
| --- | --- | --- |
| `RouterOS:ReadOnly` | `true` | Block all write/modify/delete tools |
| `RouterOS:AllowArbitraryCommands` | `false` | Expose `run_command` SSH shell |
| `RouterOS:AllowArbitraryCommandsInReadOnly` | `false` | Allow `run_command` even when read-only |
| `RouterOS:CommandDenyList` | reset/shutdown/file-remove/user-remove | Substring filter on `run_command` |
| `RouterOS:EnableRestApi` | `true` | Use `/rest` over HTTPS for structured calls |
| `RouterOS:RestAllowSelfSignedCert` | `true` | RouterOS ships a self-signed cert by default |
| `RouterOS:Enable*Tools` | `true` | Per-area kill switches |

## Tool surface (MCP)

Each entry below is one MCP tool group. Use your MCP client to discover the full schemas.

- **System** — `system_identity`, `system_resource`, `system_routerboard`, `system_health`,
  `system_clock`, `system_license`, `system_package_list`, `system_history`, `system_reboot`
- **Interfaces** — `interface_list`, `interface_ethernet_list`, `interface_vlan_list`,
  `interface_bridge_list`, `interface_bridge_port_list`, `interface_bonding_list`,
  `interface_monitor_traffic`, `interface_print_stats`, `interface_enable`, `interface_disable`
- **IP / IPv6** — `ip_address_list`, `ip_route_list`, `ip_arp_list`, `ip_neighbor_list`,
  `ip_dns_settings`, `ip_dns_cache`, `ip_dhcp_server_list`, `ip_dhcp_lease_list`,
  `ip_dhcp_client_list`, `ip_pool_list`, `ip_service_list`, `ipv6_address_list`,
  `ipv6_route_list`
- **Firewall** — `firewall_filter_list`, `firewall_nat_list`, `firewall_mangle_list`,
  `firewall_raw_list`, `firewall_address_list`, `firewall_connections`,
  `firewall_ipv6_filter_list`, `firewall_enable_rule`, `firewall_disable_rule`
- **Wireless / CAPsMAN** — `wireless_interface_list`, `wireless_registration_table`,
  `wifi_interface_list`, `wifi_registration_table`, `capsman_interface_list`,
  `capsman_registration_table`, `wireless_scan`
- **Routing** — `routing_bgp_session_list`, `routing_bgp_peer_list`,
  `routing_ospf_neighbors`, `routing_ospf_instance_list`, `routing_table_list`,
  `routing_filter_rule_list`
- **Diagnostics** — `diag_ping`, `diag_traceroute`, `diag_resolve_dns`, `diag_torch_snapshot`
- **Logs** — `log_print`
- **Management** — `user_list`, `user_active_list`, `queue_simple_list`, `queue_tree_list`,
  `file_list`, `certificate_list`, `snmp_settings`
- **Escape hatch** — `run_command` (SSH), `run_rest` (raw REST)

## MCP client connection

Add an MCP server entry pointing at the HTTP endpoint:

```jsonc
{
  "mcpServers": {
    "routeros": {
      "url": "http://localhost:5707/mcp"
    }
  }
}
```
