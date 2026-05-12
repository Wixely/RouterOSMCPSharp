using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RouterOSMCPSharp.Configuration;

namespace RouterOSMCPSharp.Services;

/// <summary>
/// Wrapper around RouterOS v7+ REST API. The REST endpoint mirrors the CLI tree:
///   GET https://router/rest/interface
///   POST https://router/rest/ip/firewall/filter   (body = JSON)
/// Print actions return arrays of objects; action endpoints (e.g. /ping) return per-call data.
/// </summary>
public sealed class RouterOSRestClient
{
    private readonly RouterOSOptions _options;
    private readonly ILogger<RouterOSRestClient> _logger;
    private readonly Lazy<HttpClient> _http;

    public RouterOSRestClient(IOptions<RouterOSOptions> options, ILogger<RouterOSRestClient> logger)
    {
        _options = options.Value;
        _logger = logger;
        _http = new Lazy<HttpClient>(CreateHttpClient);
    }

    public bool Enabled => _options.EnableRestApi;

    public async Task<JsonElement> GetAsync(string path, CancellationToken ct = default)
    {
        EnsureEnabled();
        using var resp = await _http.Value.GetAsync(NormalisePath(path), ct).ConfigureAwait(false);
        return await ReadJsonAsync(resp, ct).ConfigureAwait(false);
    }

    public async Task<JsonElement> PostAsync(string path, object? body, CancellationToken ct = default)
    {
        EnsureEnabled();
        HttpContent content = body is null
            ? new StringContent("{}", Encoding.UTF8, "application/json")
            : JsonContent.Create(body);
        try
        {
            using var resp = await _http.Value.PostAsync(NormalisePath(path), content, ct).ConfigureAwait(false);
            return await ReadJsonAsync(resp, ct).ConfigureAwait(false);
        }
        finally
        {
            content.Dispose();
        }
    }

    public async Task<JsonElement> PatchAsync(string path, object body, CancellationToken ct = default)
    {
        EnsureEnabled();
        using var content = JsonContent.Create(body);
        using var req = new HttpRequestMessage(HttpMethod.Patch, NormalisePath(path)) { Content = content };
        using var resp = await _http.Value.SendAsync(req, ct).ConfigureAwait(false);
        return await ReadJsonAsync(resp, ct).ConfigureAwait(false);
    }

    public async Task<JsonElement> DeleteAsync(string path, CancellationToken ct = default)
    {
        EnsureEnabled();
        using var resp = await _http.Value.DeleteAsync(NormalisePath(path), ct).ConfigureAwait(false);
        return await ReadJsonAsync(resp, ct).ConfigureAwait(false);
    }

    private void EnsureEnabled()
    {
        if (!_options.EnableRestApi)
            throw new InvalidOperationException("RouterOS REST API is disabled. Set RouterOS:EnableRestApi=true.");
    }

    private static string NormalisePath(string path) => path.StartsWith('/') ? path[1..] : path;

    private async Task<JsonElement> ReadJsonAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        var raw = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("RouterOS REST {Status} from {Url}: {Body}", (int)resp.StatusCode, resp.RequestMessage?.RequestUri, raw);
            throw new HttpRequestException($"RouterOS REST {(int)resp.StatusCode}: {raw}");
        }
        if (string.IsNullOrWhiteSpace(raw))
            return JsonDocument.Parse("null").RootElement;
        return JsonDocument.Parse(raw).RootElement;
    }

    private HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler();
        if (_options.RestAllowSelfSignedCert)
        {
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        var baseUrl = _options.RestBaseUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            var scheme = _options.UseHttpsForRest ? "https" : "http";
            baseUrl = $"{scheme}://{_options.Host}/rest/";
        }
        if (!baseUrl.EndsWith('/')) baseUrl += "/";

        var client = new HttpClient(handler, disposeHandler: true)
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(_options.RestTimeoutSeconds),
        };

        var creds = $"{_options.Username}:{_options.Password}";
        var basic = Convert.ToBase64String(Encoding.ASCII.GetBytes(creds));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("RouterOSMCPSharp/1.0");
        return client;
    }
}
