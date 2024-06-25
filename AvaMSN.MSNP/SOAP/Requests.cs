using System.Text;

namespace AvaMSN.MSNP.SOAP;

/// <summary>
/// Static class to make SOAP requests.
/// </summary>
public static class Requests
{
    private static readonly HttpClient HttpClient = new();

    /// <summary>
    /// Makes a SOAP request.
    /// </summary>
    /// <param name="soapXml">XML body content.</param>
    /// <param name="url">URL to request to.</param>
    /// <param name="soapAction">SOAP action header content.</param>
    /// <returns>Response as a string.</returns>
    public static async Task<string> MakeRequest(string soapXml, string url, string soapAction)
    {
        using (HttpContent content = new StringContent(soapXml, Encoding.UTF8, "text/xml"))
        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url))
        {
            request.Headers.Add("SOAPAction", soapAction);
            request.Content = content;

            using (HttpResponseMessage response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}