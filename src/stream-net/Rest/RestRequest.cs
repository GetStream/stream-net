﻿using System;
using System.Collections.Generic;

namespace Stream.Rest
{
    internal class RestRequest
    {
        public class Parameter
        {
            public string Name { get; set; }

            public string Value { get; set; }
        }

        private IDictionary<string, string> _headers = new Dictionary<string, string>();
        private IDictionary<string, string> _queryParameters = new Dictionary<string, string>();

        internal RestRequest(string resource, HttpMethod method)
        {
            Method = method;
            Resource = resource;
        }

        public HttpMethod Method { get; private set; }

        public string Resource { get; private set; }

        public string JsonBody { get; private set; }
        public System.IO.Stream FileStream { get; private set; }
        public string FileStreamContentType { get; private set; }

        public void AddHeader(string name, string value)
        {
            _headers[name] = value;
        }

        public void AddQueryParameter(string name, string value)
        {
            _queryParameters[name] = value;
        }

        public void SetJsonBody(string json)
        {
            JsonBody = json;
        }

        public void SetFileStream(System.IO.Stream stream, string contentType)
        {
          FileStream = stream;
          FileStreamContentType = contentType;
        }

        public IEnumerable<KeyValuePair<string, string>> QueryParameters {
            get
            {
                return _queryParameters;
            }
        }

        public IEnumerable<KeyValuePair<string,string>> Headers
        {
            get
            {
                return _headers;
            }
        }

        public bool HasBody
        {
            get { return JsonBody != null; }
        }
    }
}
