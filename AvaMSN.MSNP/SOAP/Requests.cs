using System.Text;
using Serilog;

namespace AvaMSN.MSNP.SOAP;

/// <summary>
/// Static class to make SOAP requests.
/// </summary>
internal static class Requests
{
    private static readonly HttpClient HttpClient = new HttpClient();

    /// <summary>
    /// Makes a SOAP request.
    /// </summary>
    /// <param name="soapXml">XML body content.</param>
    /// <param name="url">URL to request to.</param>
    /// <param name="soapAction">SOAP action header content.</param>
    /// <param name="logRequest">Whether to log the request content. Defaults to true.</param>
    /// <returns>Response as a string.</returns>
    public static async Task<string> MakeRequest(string soapXml, string url, string soapAction, bool logRequest = true)
    {
        using (HttpContent content = new StringContent(soapXml, Encoding.UTF8, "text/xml"))
        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url))
        {
            request.Headers.Add("SOAPAction", soapAction);
            request.Content = content;
            if (logRequest)
                Log.Information("Sent SOAP: {request}", await content.ReadAsStringAsync());

            using (HttpResponseMessage response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                string reply = await response.Content.ReadAsStringAsync();
                reply = reply.Trim();
                Log.Information("Received SOAP: {reply}", reply);
                return reply;
            }
        }
    }
}