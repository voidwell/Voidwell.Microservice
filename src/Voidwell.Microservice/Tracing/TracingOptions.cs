using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Voidwell.Microservice.Tracing
{
    public class TracingOptions
    {
        public List<Func<HttpContext, bool>> IgnoreTrace { get; set; } = new List<Func<HttpContext, bool>>();
    }
}
