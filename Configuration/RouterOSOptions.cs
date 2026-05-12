namespace RouterOSMCPSharp.Configuration;

public sealed class RouterOSOptions
{
    public const string SectionName = "RouterOS";

    /// <summary>Hostname or IP address of the RouterOS device.</summary>
    public string Host { get; set; } = "192.168.88.1";

    /// <summary>SSH port. RouterOS default is 22.</summary>
    public int SshPort { get; set; } = 22;

    /// <summary>Username for SSH and REST authentication.</summary>
    public string Username { get; set; } = "admin";

    /// <summary>Password for SSH and REST authentication. Prefer PrivateKeyPath for SSH.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Optional path to an OpenSSH-format private key for SSH auth. Takes precedence over Password.</summary>
    public string? PrivateKeyPath { get; set; }

    /// <summary>Optional passphrase for the private key.</summary>
    public string? PrivateKeyPassphrase { get; set; }

    /// <summary>Expected SSH host key fingerprint (SHA256 base64). When set, mismatches abort the connection.</summary>
    public string? ExpectedHostKeyFingerprint { get; set; }

    /// <summary>SSH connect/keepalive timeout in seconds.</summary>
    public int SshTimeoutSeconds { get; set; } = 30;

    /// <summary>SSH command execution timeout in seconds.</summary>
    public int SshCommandTimeoutSeconds { get; set; } = 60;

    /// <summary>Enable use of the RouterOS v7+ REST API for structured queries (HTTP/HTTPS on the device).</summary>
    public bool EnableRestApi { get; set; } = true;

    /// <summary>REST API base URL, e.g. "https://192.168.88.1/rest". Leave null to derive from Host + UseHttpsForRest.</summary>
    public string? RestBaseUrl { get; set; }

    /// <summary>If RestBaseUrl is null, use HTTPS (true) or HTTP (false) when deriving the REST URL.</summary>
    public bool UseHttpsForRest { get; set; } = true;

    /// <summary>If true, accept any TLS certificate from the RouterOS device. RouterOS ships with a self-signed cert.</summary>
    public bool RestAllowSelfSignedCert { get; set; } = true;

    /// <summary>REST request timeout in seconds.</summary>
    public int RestTimeoutSeconds { get; set; } = 30;

    /// <summary>When true (default), all write/modify/delete tools and arbitrary command execution are blocked.</summary>
    public bool ReadOnly { get; set; } = true;

    /// <summary>When true, exposes the run_command tool for arbitrary CLI execution. Off by default; ignored when ReadOnly=true unless AllowArbitraryCommandsInReadOnly is also true.</summary>
    public bool AllowArbitraryCommands { get; set; } = false;

    /// <summary>When true, allow arbitrary commands even in read-only mode. Use with care.</summary>
    public bool AllowArbitraryCommandsInReadOnly { get; set; } = false;

    /// <summary>Optional substring deny-list for arbitrary commands (case-insensitive). Commands containing any of these strings are rejected.</summary>
    public List<string> CommandDenyList { get; set; } = new()
    {
        "/system reset-configuration",
        "/system shutdown",
        "/file remove",
        "/user remove",
    };

    /// <summary>Maximum number of rows returned by list operations.</summary>
    public int DefaultPageSize { get; set; } = 200;

    /// <summary>Maximum log entries to fetch in a single call.</summary>
    public int MaxLogEntries { get; set; } = 500;

    /// <summary>Enable system + resource tools (identity, clock, board, RouterBOOT, license, health).</summary>
    public bool EnableSystemTools { get; set; } = true;

    /// <summary>Enable interface tools (ethernet, vlan, bonding, bridge, monitor-traffic, statistics).</summary>
    public bool EnableInterfaceTools { get; set; } = true;

    /// <summary>Enable IP/IPv6 tools (addresses, routes, DNS, DHCP server/client, ARP, neighbours).</summary>
    public bool EnableIpTools { get; set; } = true;

    /// <summary>Enable firewall tools (filter, NAT, mangle, raw, address-list, connection table).</summary>
    public bool EnableFirewallTools { get; set; } = true;

    /// <summary>Enable wireless / CAPsMAN tools (interfaces, registration table, scan).</summary>
    public bool EnableWirelessTools { get; set; } = true;

    /// <summary>Enable routing tools (BGP, OSPF, RIP, route-filters).</summary>
    public bool EnableRoutingTools { get; set; } = true;

    /// <summary>Enable diagnostic tools (ping, traceroute, bandwidth-test, torch-snapshot).</summary>
    public bool EnableDiagnosticsTools { get; set; } = true;

    /// <summary>Enable log tools.</summary>
    public bool EnableLogTools { get; set; } = true;

    /// <summary>Enable user/queue/file tools.</summary>
    public bool EnableManagementTools { get; set; } = true;
}
