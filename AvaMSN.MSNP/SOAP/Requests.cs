using System.Text;

namespace AvaMSN.MSNP.SOAP;

public static class Requests
{
    public static readonly HttpClient HttpClient = new();

    public static async Task<string> SoapRequest(string soapXml, string url, string soapAction)
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