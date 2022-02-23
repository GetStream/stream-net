﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Stream.Rest
{
    internal class RestResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public string Content { get; set; }

        internal static async Task<RestResponse> FromResponseMessage(HttpResponseMessage message)
        {
            var response = new RestResponse { StatusCode = message.StatusCode };

            using (message)
            {
                response.Content = await message.Content.ReadAsStringAsync();
            }

            return response;
        }
    }
}