namespace RouterOSMCPSharp.Configuration;

public sealed class ServerOptions
{
    public const string SectionName = "Server";

    /// <summary>Host the MCP HTTP listener binds to. Use 0.0.0.0 inside Docker.</summary>
    public string Host { get; set; } = "0.0.0.0";

    /// <summary>Port the MCP HTTP listener binds to.</summary>
    public int Port { get; set; } = 5100;

    /// <summary>HTTP path the MCP endpoint is mounted at.</summary>
    public string Path { get; set; } = "/mcp";

    /// <summary>Service name when running as a Windows Service.</summary>
    public string WindowsServiceName { get; set; } = "RouterOSMCPSharp";
}
