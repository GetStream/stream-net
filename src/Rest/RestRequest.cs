using System.Collections.Generic;
using System.Net.Http;

namespace Stream.Rest
{
    internal class RestRequest
    {
        internal RestRequest(string resource, HttpMethod method)
        {
            Method = method;
            Resource = resource;
        }

        public Dictionary<string, string> QueryParameters { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Headers { get; private set; } = new Dictionary<string, string>();
        public HttpMethod Method { get; private set; }
        public string Resource { get; private set; }
        public string JsonBody { get; private set; }
        public System.IO.Stream FileStream { get; private set; }
        public string FileStreamContentType { get; private set; }
        public string FileStreamName { get; private set; }

        public void AddHeader(string name, string value) => Headers[name] = value;

        public void AddQueryParameter(string name, string value) => QueryParameters[name] = value;

        public void SetJsonBody(string json) => JsonBody = json;

        public void SetFileStream(System.IO.Stream stream, string name, string contentType)
        {
            FileStream = stream;
            FileStreamName = name;
            FileStreamContentType = contentType;
        }
    }
}
