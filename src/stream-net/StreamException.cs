using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stream
{
    public class StreamException : Exception
    {
        public static StreamException FromResponse(IRestResponse response)
        {
            throw new StreamException();
        }
    }
}
