using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using NSubstitute;

namespace RX.Nyss.FuncApp.Tests
{
    internal sealed class FakeHttpRequestData : HttpRequestData
    {
        public FakeHttpRequestData(FunctionContext functionContext, string body)
            : base(functionContext)
        {
            var bytes = Encoding.UTF8.GetBytes(body ?? string.Empty);
            Body = new MemoryStream(bytes);
            Headers = new HttpHeadersCollection();
            Cookies = new List<IHttpCookie>();
            Url = new Uri("https://localhost/api/enqueueNyssReport");
            Identities = Array.Empty<ClaimsIdentity>();
            Method = "POST";
        }

        public override Stream Body { get; }

        public override HttpHeadersCollection Headers { get; }

        public override IReadOnlyCollection<IHttpCookie> Cookies { get; }

        public override Uri Url { get; }

        public override IEnumerable<ClaimsIdentity> Identities { get; }

        public override string Method { get; }

        public override HttpResponseData CreateResponse() => new FakeHttpResponseData(FunctionContext);
    }

    internal sealed class FakeHttpResponseData : HttpResponseData
    {
        public FakeHttpResponseData(FunctionContext functionContext)
            : base(functionContext)
        {
            Headers = new HttpHeadersCollection();
            Body = new MemoryStream();
            Cookies = Substitute.For<HttpCookies>();
        }

        public override HttpStatusCode StatusCode { get; set; }

        public override HttpHeadersCollection Headers { get; set; }

        public override Stream Body { get; set; }

        public override HttpCookies Cookies { get; }
    }
}
