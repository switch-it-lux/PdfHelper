using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;

namespace Sitl.Pdf {

    // Resource retriever used by pdfHTML when converting untrusted HTML (e.g. emails) to PDF.
    // Remote resources (images, stylesheets, fonts) are still downloaded, but only when they are safe to fetch:
    // - Only absolute http/https URLs on default ports (80/443): file://, UNC paths and relative URLs are blocked
    //   (prevents local file inclusion and NTLM hash leaks through SMB).
    // - The host must only resolve to public IP addresses: loopback, private, link-local... addresses are blocked
    //   (prevents SSRF against the internal network or cloud metadata endpoints).
    // - Redirects are followed manually and each target is checked again (a public URL could redirect to an internal one).
    // - Size, count and time limits prevent a crafted HTML from overloading the server.
    // These checks follow the OWASP SSRF prevention cheat sheet (application layer, "Case 2"):
    // https://cheatsheetseries.owasp.org/cheatsheets/Server_Side_Request_Forgery_Prevention_Cheat_Sheet.html
    // Inline "data:" resources are resolved by iText itself and do not go through this retriever.
    // Known limitation: the host is resolved before the request, so a DNS server answering differently
    // between the check and the request (DNS rebinding) is not fully covered.
    // Both retriever interfaces are implemented: iText.StyledXmlParser one is obsolete in favor of iText.IO one,
    // but pdfHTML ConverterProperties.SetResourceRetriever() still only accepts the obsolete one.
#pragma warning disable CS0618 // Remove the obsolete interface once ConverterProperties accepts iText.IO.Resolver.Resource.IResourceRetriever
    internal class SafeResourceRetriever : iText.IO.Resolver.Resource.IResourceRetriever, iText.StyledXmlParser.Resolver.Resource.IResourceRetriever {
#pragma warning restore CS0618
        private const int MaxResourceSize = 5 * 1024 * 1024;
        private const long MaxTotalSize = 20 * 1024 * 1024;
        private const int MaxResourceCount = 200;
        private const int MaxRedirects = 5;
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan TotalTimeout = TimeSpan.FromSeconds(30);

        // HttpClient is shared (recommended usage), without automatic redirects and with a response size limit.
        private static readonly HttpClient Client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) {
            Timeout = RequestTimeout,
            MaxResponseContentBufferSize = MaxResourceSize
        };

        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private int _resourceCount;
        private long _totalSize;

        public Stream GetInputStreamByUrl(Uri url) {
            var bytes = GetByteArrayByUrl(url);
            return bytes != null ? new MemoryStream(bytes) : null;
        }

        public byte[] GetByteArrayByUrl(Uri url) {
            // Returning null makes pdfHTML skip the resource (the conversion goes on without it).
            try {
                if (!IsAllowedUrl(url)) return null;
                if (_resourceCount >= MaxResourceCount || _totalSize >= MaxTotalSize || _stopwatch.Elapsed >= TotalTimeout) return null;
                _resourceCount++;

                for (int i = 0; i <= MaxRedirects; i++) {
                    using (var response = Client.GetAsync(url).ConfigureAwait(false).GetAwaiter().GetResult()) {
                        if (IsRedirect(response.StatusCode)) {
                            // Redirect target (often http to https) must pass the same checks as the original URL
                            if (response.Headers.Location == null) return null;
                            url = response.Headers.Location.IsAbsoluteUri ? response.Headers.Location : new Uri(url, response.Headers.Location);
                            if (!IsAllowedUrl(url)) return null;
                            continue;
                        }

                        if (!response.IsSuccessStatusCode) return null;
                        var bytes = response.Content.ReadAsByteArrayAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        _totalSize += bytes.Length;
                        return bytes;
                    }
                }
                return null;
            } catch {
                return null;
            }
        }

        private static bool IsRedirect(HttpStatusCode statusCode) {
            var code = (int)statusCode;
            return code == 301 || code == 302 || code == 303 || code == 307 || code == 308;
        }

        internal static bool IsAllowedUrl(Uri url) {
            if (url == null || !url.IsAbsoluteUri) return false;
            if (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps) return false;
            if (!url.IsDefaultPort) return false;

            // Every address the host resolves to must be public, as the HTTP client may use any of them.
            IPAddress[] addresses;
            if (IPAddress.TryParse(url.DnsSafeHost, out var ip)) addresses = new[] { ip };
            else addresses = Dns.GetHostAddresses(url.DnsSafeHost);

            return addresses.Length > 0 && addresses.All(IsPublicAddress);
        }

        internal static bool IsPublicAddress(IPAddress ip) {
            if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
            if (IPAddress.IsLoopback(ip)) return false;

            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                var b = ip.GetAddressBytes();
                if (b[0] == 0) return false;                                // 0.0.0.0/8 (this network)
                if (b[0] == 10) return false;                               // 10.0.0.0/8 (private)
                if (b[0] == 100 && b[1] >= 64 && b[1] <= 127) return false; // 100.64.0.0/10 (carrier-grade NAT)
                if (b[0] == 169 && b[1] == 254) return false;               // 169.254.0.0/16 (link-local, cloud metadata)
                if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return false;  // 172.16.0.0/12 (private)
                if (b[0] == 192 && b[1] == 0 && b[2] == 0) return false;    // 192.0.0.0/24 (IETF protocol assignments)
                if (b[0] == 192 && b[1] == 168) return false;               // 192.168.0.0/16 (private)
                if (b[0] == 198 && (b[1] == 18 || b[1] == 19)) return false;// 198.18.0.0/15 (benchmarking)
                if (b[0] >= 224) return false;                              // 224.0.0.0/4 multicast, 240.0.0.0/4 reserved, broadcast
                return true;
            }

            if (ip.AddressFamily == AddressFamily.InterNetworkV6) {
                if (ip.Equals(IPAddress.IPv6Any) || ip.Equals(IPAddress.IPv6None)) return false;
                if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6Multicast) return false;
                var b = ip.GetAddressBytes();
                if ((b[0] & 0xFE) == 0xFC) return false;                    // fc00::/7 (unique local)
                return true;
            }

            return false;
        }
    }
}
