using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using RouterOSMCPSharp.Services;

namespace RouterOSMCPSharp.Tools;

[McpServerToolType]
public static class WirelessTools
{
    [McpServerTool(Name = "wireless_interface_list"),
     Description("Wireless interfaces (legacy /interface wireless tree, present on RouterOS <7.13 builds).")]
    public static Task<string> InterfaceList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/interface/wireless", ct);
    }

    [McpServerTool(Name = "wireless_registration_table"),
     Description("Currently associated wireless clients with signal, RX/TX rates and uptime.")]
    public static Task<string> RegistrationTable(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/interface/wireless/registration-table", ct);
    }

    [McpServerTool(Name = "wifi_interface_list"),
     Description("New /interface/wifi tree (RouterOS 7.13+ wifi-qcom driver).")]
    public static Task<string> WifiList(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/interface/wifi", ct);
    }

    [McpServerTool(Name = "wifi_registration_table"),
     Description("Associated wifi clients on the new wifi driver.")]
    public static Task<string> WifiRegistrationTable(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/interface/wifi/registration-table", ct);
    }

    [McpServerTool(Name = "capsman_interface_list"),
     Description("CAPsMAN provisioned interfaces.")]
    public static Task<string> CapsmanInterfaces(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/caps-man/interface", ct);
    }

    [McpServerTool(Name = "capsman_registration_table"),
     Description("Clients connected via CAPsMAN.")]
    public static Task<string> CapsmanRegistrationTable(RouterOSService svc, CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        return ToolHelpers.PrintAsync(svc, "/caps-man/registration-table", ct);
    }

    [McpServerTool(Name = "wireless_scan"),
     Description("One-shot scan of nearby SSIDs on a wireless interface. Times out at SshCommandTimeoutSeconds.")]
    public static async Task<string> Scan(
        RouterOSService svc,
        [Description("Wireless interface name (e.g. wlan1).")] string name,
        [Description("Scan duration in seconds (default 5).")] int durationSeconds = 5,
        CancellationToken ct = default)
    {
        EnsureEnabled(svc);
        var result = await svc.Ssh.ExecuteAsync($"/interface wireless scan {name} duration={durationSeconds}", ct);
        return JsonSerializer.Serialize(new { stdout = result.Output, stderr = result.Error, exitStatus = result.ExitStatus }, JsonOpts.Default);
    }

    private static void EnsureEnabled(RouterOSService svc)
    {
        if (!svc.Options.EnableWirelessTools)
            throw new InvalidOperationException("Wireless tools are disabled (RouterOS:EnableWirelessTools=false).");
    }
}
