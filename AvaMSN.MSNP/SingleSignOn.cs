using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using AvaMSN.MSNP.SOAP;
using AvaMSN.MSNP.SOAP.RequestObjects;
using Serilog;

namespace AvaMSN.MSNP;

/// <summary>
/// Contains functions used in SSO authentication and stores auth data.
/// </summary>
public class SingleSignOn
{
    public string BinarySecret { get; set; } = string.Empty;
    public string Ticket { get; set; } = string.Empty;
    public string TicketToken { get; set; } = string.Empty;

    public string RstAddress { get; } = string.Empty;

    public SingleSignOn(string host) 
    {
        RstAddress = $"https://{host}/RST.srf";
    }

    /// <summary>
    /// Converts an array of uints into a byte array.
    /// </summary>
    /// <param name="uintArray">Array of uints.</param>
    /// <returns>Converted byte array.</returns>
    public static byte[] UIntBytes(uint[] uintArray)
    {
        byte[] bytes = new byte[sizeof(uint) * uintArray.Length];
        byte[] indexBytes;

        for (int i = 0; i < uintArray.Length; i++)
        {
            indexBytes = BitConverter.GetBytes(uintArray[i]);
            Buffer.BlockCopy(indexBytes, 0, bytes, i * sizeof(uint), sizeof(uint));
        }

        return bytes;
    }

    /// <summary>
    /// Makes SOAP request to RST url and gets necessary strings from the response.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <param name="password">User password.</param>
    /// <returns></returns>
    public async Task RstRequest(string username, string password)
    {
        XmlSerializer requestSerializer = new(typeof(SOAP.SerializableClasses.RST.Envelope));

        var envelope = RequestObjects.RSTRequest();
        envelope.Header.Security.UsernameToken.Username = username;
        envelope.Header.Security.UsernameToken.Password = password;

        using MemoryStream memory = new();
        requestSerializer.Serialize(memory, envelope);

        Log.Information("Making SOAP request {Request} to {Url}", "RST", RstAddress);
        string soapXML = Encoding.UTF8.GetString(memory.ToArray());
        string response = await Requests.MakeRequest(soapXML, RstAddress, "http://www.msn.com/webservices/storage/w10/");

        XmlSerializer responseSerializer = new(typeof(SOAP.SerializableClasses.RstResponse.Envelope));
        using (StringReader reader = new StringReader(response))
        {
            var responseEnvelope = (SOAP.SerializableClasses.RstResponse.Envelope?)responseSerializer.Deserialize(reader);
            Ticket = responseEnvelope!.Body.RequestSecurityTokenResponseCollection[1].RequestedSecurityToken.BinarySecurityToken.Value;
            BinarySecret = responseEnvelope.Body.RequestSecurityTokenResponseCollection[1].RequestedProofToken.BinarySecret;
            TicketToken = responseEnvelope.Body.RequestSecurityTokenResponseCollection[2].RequestedSecurityToken.BinarySecurityToken.Value;
        }
    }

    /// <summary>
    /// Hashes key with WS-Secure string. Used during return value calculation.
    /// </summary>
    /// <param name="key">Base64 binary secret.</param>
    /// <param name="wsSecure">WS-Secure string.</param>
    /// <returns>Hashed key bytes.</returns>
    private static byte[] WsKey(byte[] key, string wsSecure)
    {
        HMACSHA1 hMACSHA1 = new HMACSHA1(key);
        byte[] wsSecureBytes = Encoding.UTF8.GetBytes(wsSecure);

        byte[] hash1 = hMACSHA1.ComputeHash(wsSecureBytes);
        byte[] hash2 = hMACSHA1.ComputeHash(hash1.Concat(wsSecureBytes).ToArray());
        byte[] hash3 = hMACSHA1.ComputeHash(hash1);
        byte[] hash4 = hMACSHA1.ComputeHash(hash3.Concat(wsSecureBytes).ToArray());

        byte[] hash4Fourbytes = new byte[4];
        Buffer.BlockCopy(hash4, 0, hash4Fourbytes, 0, hash4Fourbytes.Length);
        
        byte[] returnKey = hash2.Concat(hash4Fourbytes).ToArray();
        return returnKey;
    }

    /// <summary>
    /// Calculates the SSO return value used in authentication.
    /// </summary>
    /// <param name="nonce">Nonce obtained from server response.</param>
    /// <returns>SSO return value.</returns>
    public string GetReturnValue(string nonce)
    {
        byte[] nonceBytes = Encoding.UTF8.GetBytes(nonce);

        byte[] key1 = Convert.FromBase64String(BinarySecret);
        byte[] key2 = WsKey(key1, "WS-SecureConversationSESSION KEY HASH");
        byte[] key3 = WsKey(key1, "WS-SecureConversationSESSION KEY ENCRYPTION");

        HMACSHA1 hMACSHA1 = new HMACSHA1(key2);
        byte[] key2Hash = hMACSHA1.ComputeHash(nonceBytes);
        byte[] eight8Bytes = { 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08 };
        byte[] paddedNonce = nonceBytes.Concat(eight8Bytes).ToArray();
        byte[] randomBytes = RandomNumberGenerator.GetBytes(8);

        TripleDES tripleDES = TripleDES.Create();
        tripleDES.Mode = CipherMode.CBC;
        byte[] encryptedData = new byte[72];
        tripleDES.CreateEncryptor(key3, randomBytes).TransformBlock(paddedNonce, 0, paddedNonce.Length, encryptedData, 0);

        uint[] headerValues =
        {
            28,//uStructHeaderSize
            1,//uCryptMode
            0x6603,//uCipherMode
            0x8004,//uHashType
            8,//uIVLen
            20,//uHashLen
            72//uCipherLen
        };

        byte[] returnStruct = UIntBytes(headerValues);
        returnStruct = returnStruct.Concat(randomBytes).ToArray();//aIVBytes
        returnStruct = returnStruct.Concat(key2Hash).ToArray();//aHashBytes
        returnStruct = returnStruct.Concat(encryptedData).ToArray();//aCipherBytes
        return Convert.ToBase64String(returnStruct);
    }
}
