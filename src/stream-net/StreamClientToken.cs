using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Stream
{
    public interface IToken
    {
        string CreateUserToken(string userId, IDictionary<string, object> extraData = null);

        string For(object payload);
    }

    public static class TokenFactory
    {
        public static IToken For(string apiSecretOrToken)
        {
            return apiSecretOrToken.Contains(".")
                ? (IToken)new Token(apiSecretOrToken)
                : (IToken)new Secret(apiSecretOrToken);
        }
    }

    public class Token : IToken
    {
        private readonly string _sessionToken;

        public Token(string sessionToken)
        {
            _sessionToken = sessionToken;
        }

        public string CreateUserToken(string userId, IDictionary<string, object> extraData = null)
        {
            throw new InvalidOperationException("Clients connecting using a user token cannot create additional user tokens");
        }

        public string For(object payload)
        {
            return _sessionToken;
        }
    }

    public class Secret : IToken
    {
        private readonly string _apiSecret;

        public Secret(string apiSecret)
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

        public string CreateUserToken(string userId, IDictionary<string, object> extraData = null)
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
