using System;
using System.Net.Http.Headers;
using System.Text;

namespace RX.Nyss.Common.Services.EidsrClient.Dto;

public class BasicAuthenticationHeaderValue : AuthenticationHeaderValue
{
    public BasicAuthenticationHeaderValue(string authorizationHeader)
        : base("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(authorizationHeader)))
    {
    }
}
