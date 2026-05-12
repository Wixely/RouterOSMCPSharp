# RouterOSMCPSharp

A Model Context Protocol (MCP) server for Mikrotik **RouterOS**. Exposes a broad surface
of RouterOS features so an MCP-aware agent can read statistics, diagnose issues, and
(when permitted) make changes.

> Status: scaffolding. Tools cover system, interfaces, IP/IPv6, firewall, wireless, routing,
> diagnostics, logs, management and an arbitrary-command escape hatch (off by default).

## Highlights

- **Two transports to the device** — first-class RouterOS v7+ REST API (HTTPS) for
  structured JSON, with SSH as a fallback and the channel for arbitrary commands.
  No proprietary tricks; both are official RouterOS interfaces.
- **Read-only by default** — every write/modify/delete tool is gated behind
  `RouterOS:ReadOnly=false`.
- **Arbitrary commands are double-gated** — `RouterOS:AllowArbitraryCommands=true` plus
  either non-read-only mode or `AllowArbitraryCommandsInReadOnly=true`, with a
  configurable substring deny-list.
- **Runs anywhere** — Docker first (recommended), Windows Service, or plain console.
- **Releases** — tag `vX.Y.Z` and the GitHub workflow publishes win-x64 / linux-x64 /
  linux-arm64 zips (framework-dependent and self-contained) plus a multi-arch
  `linux/amd64,linux/arm64` Docker image to GHCR.

## Quick start

### Docker (recommended)

```bash
docker run --rm -it \
  -p 5100:5100 \
  -v $(pwd)/logs:/app/logs \
  -e RouterOS__Host=192.168.88.1 \
  -e RouterOS__Username=admin \
  -e RouterOS__Password=changeme \
  ghcr.io/wixely/routerosmcpsharp:latest
```

The MCP HTTP endpoint is then available at `http://localhost:5100/mcp`.

### Standalone

```bash
dotnet run --project RouterOSMCPSharp.csproj
```

Edit [appsettings.json](appsettings.json) (or use environment variables) to point
at your router. Local-only overrides go in `appsettings.Local.json`, which is
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

All settings live under the `RouterOS` and `Server` sections of `appsettings.json`.
Override any field with environment variables using the standard double-underscore
convention, e.g. `RouterOS__Password`, `Server__Port`.

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

Each entry below is one MCP tool. Use the MCP client of your choice
(Claude Desktop, MCP Inspector, etc.) to discover the full schemas.

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

## Connecting from Claude

Add an MCP server entry pointing at the HTTP endpoint:

```jsonc
{
  "mcpServers": {
    "routeros": {
      "url": "http://localhost:5100/mcp"
    }
  }
}
```

## Developing

```bash
dotnet build
dotnet run
```

VS Code: open the folder, hit F5. Visual Studio: open `RouterOSMCPSharp.sln`,
F5. Both honour `Properties/launchSettings.json`.

## Releases

Push a tag matching `v*` to trigger [.github/workflows/build-release-packages.yml](.github/workflows/build-release-packages.yml):

1. Build & test on the configured matrix (`win-x64`, `linux-x64`, `linux-arm64`,
   each as framework-dependent and self-contained).
2. Build a multi-arch (`linux/amd64,linux/arm64`) Docker image and push it to
   `ghcr.io/<repo>:<version>` and `:latest`.
3. Create a GitHub Release with the zips attached.
