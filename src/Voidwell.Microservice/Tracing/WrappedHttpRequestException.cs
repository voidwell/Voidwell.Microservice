using System;
using System.Net.Http;

namespace Voidwell.Microservice.Tracing
{
    public class WrappedHttpRequestException : Exception
    {
        HttpRequestMessage Request { get; }

        public WrappedHttpRequestException(HttpRequestException innerException, HttpRequestMessage request)
            : base("HttpRequestException thrown", innerException)
        {
            Request = request;
        }

        public override string Message
        {
            get
            {
                return $"An error occurred when sending the request {Request?.Method} {Request?.RequestUri}";
            }
        }
    }
}
