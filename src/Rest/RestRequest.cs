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

        internal Dictionary<string, string> QueryParameters { get; private set; } = new Dictionary<string, string>();
        internal Dictionary<string, string> Headers { get; private set; } = new Dictionary<string, string>();
        internal HttpMethod Method { get; private set; }
        internal string Resource { get; private set; }
        internal string JsonBody { get; private set; }
        internal System.IO.Stream FileStream { get; private set; }
        internal string FileStreamContentType { get; private set; }
        internal string FileStreamName { get; private set; }

        internal void AddHeader(string name, string value) => Headers[name] = value;

        internal void AddQueryParameter(string name, string value) => QueryParameters[name] = value;

        internal void SetJsonBody(string json) => JsonBody = json;

        internal void SetFileStream(System.IO.Stream stream, string name, string contentType)
        {
            FileStream = stream;
            FileStreamName = name;
            FileStreamContentType = contentType;
        }
    }
}
