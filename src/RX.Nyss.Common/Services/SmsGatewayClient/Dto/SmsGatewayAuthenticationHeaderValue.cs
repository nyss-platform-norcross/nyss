using System;
using System.Net.Http.Headers;
using System.Text;

namespace RX.Nyss.Common.Services.SmsGatewayClient.Dto;

public class SmsGatewayAuthenticationHeaderValue : AuthenticationHeaderValue
{
    public SmsGatewayAuthenticationHeaderValue(string authorizationHeader)
        : base("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(authorizationHeader)))
    {
    }
}
