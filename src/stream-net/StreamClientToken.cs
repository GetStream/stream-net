using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Stream
{
    public interface IStreamClientToken
    {
        string CreateUserSessionToken(string userId, IDictionary<string, object> extraData = null);

        string For(object payload);
    }

    public static class StreamClientToken
    {
        public static IStreamClientToken For(string apiSecretOrToken)
        {
            return apiSecretOrToken.Contains(".")
                ? (IStreamClientToken) new StreamApiSessionToken(apiSecretOrToken)
                : (IStreamClientToken) new StreamApiSecret(apiSecretOrToken);
        }
    }

    public class StreamApiSessionToken : IStreamClientToken
    {
        private readonly string _sessionToken;

        public StreamApiSessionToken(string sessionToken)
        {
            _sessionToken = sessionToken;
        }

        public string CreateUserSessionToken(string userId, IDictionary<string, object> extraData = null)
        {
            throw new InvalidOperationException("Clients connecting using a user session token cannot create additional user session tokens");
        }

        public string For(object payload)
        {
            return _sessionToken;
        }
    }

    public class StreamApiSecret : IStreamClientToken
    {
        private readonly string _apiSecret;

        public StreamApiSecret(string apiSecret)
        {
            _apiSecret = apiSecret;
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .Trim('=');
        }

        public string CreateUserSessionToken(string userId, IDictionary<string, object> extraData = null)
        {
            var payload = new Dictionary<string, object>
            {
                {"user_id", userId}
            };
            if (extraData != null)
            {
                extraData.ForEach(x => payload[x.Key] = x.Value);
            }
            return For(payload);
        }

        public string For(object payload)
        {
            var segments = new List<string>();

            byte[] headerBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(StreamClient.JWTHeader));
            byte[] payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));

            segments.Add(Base64UrlEncode(headerBytes));
            segments.Add(Base64UrlEncode(payloadBytes));

            var stringToSign = string.Join(".", segments.ToArray());
            var bytesToSign = Encoding.UTF8.GetBytes(stringToSign);

            using (var sha = new HMACSHA256(Encoding.UTF8.GetBytes(_apiSecret)))
            {
                byte[] signature = sha.ComputeHash(bytesToSign);
                segments.Add(Base64UrlEncode(signature));
            }
            return string.Join(".", segments.ToArray());
        }
    }
}
