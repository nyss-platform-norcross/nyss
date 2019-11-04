using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Authentication;
using RX.Nyss.Web.Features.Authentication.Policies;
using Shouldly;
using Xunit;

namespace Rx.Nyss.Web.Tests.Features.Authentication.Policies
{
    public class SmsGatewayAccessHandlerTests
    {
        public static IEnumerable<object[]> RolesWithAccessToAllSmsGateways => SmsGatewayAccessHandler.RolesWithAccessToAllSmsGateways.Select(x => new object[] { x });
        
        private readonly SmsGatewayAccessHandler _smsGatewayAuthorizationHandler;
        private readonly HttpRequest _httpRequestMock;
        private readonly INyssContext _nyssContextMock;
        private readonly AuthorizationHandlerContext _authorizationHandlerContext;
        private readonly ClaimsPrincipal _userMock;
        
        public SmsGatewayAccessHandlerTests()
        {
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var httpContextMock = Substitute.For<HttpContext>();
            httpContextAccessorMock.HttpContext.Returns(httpContextMock);
            _httpRequestMock = Substitute.For<HttpRequest>();
            httpContextMock.Request.Returns(_httpRequestMock);
            
            _nyssContextMock = Substitute.For<INyssContext>();
            
            _smsGatewayAuthorizationHandler = new SmsGatewayAccessHandler(httpContextAccessorMock, _nyssContextMock);
            var smsGatewayAccessRequirement = new SmsGatewayAccessRequirement();
            var smsGatewayAccessRequirements = new[] {smsGatewayAccessRequirement};
            _userMock = Substitute.For<ClaimsPrincipal>();
            _authorizationHandlerContext = new AuthorizationHandlerContext(smsGatewayAccessRequirements, _userMock, null);
        }

        [Theory]
        [MemberData(nameof(RolesWithAccessToAllSmsGateways))]
        public async Task SmsGatewayAccessHandler_WhenUserHasRoleWithAccessToAllSmsGateways_ShouldSucceed(string roleName)
        {
            // Arrange
            var routeValueDictionary =
                new RouteValueDictionary(new Dictionary<string, string>
                {
                    [SmsGatewayAccessHandler.RouteValueName] = "2"
                });
            _httpRequestMock.RouteValues.Returns(routeValueDictionary);
            var claims = new[] { new Claim(ClaimTypes.Role, roleName) };
            _userMock.Claims.Returns(claims);

            // Act
            await _smsGatewayAuthorizationHandler.HandleAsync(_authorizationHandlerContext);

            // Assert
            _authorizationHandlerContext.HasSucceeded.ShouldBe(true);
        }

        [Fact]
        public async Task SmsGatewayAccessHandler_WhenUserHasAccessToNationalSociety_ShouldSucceed()
        {
            // Arrange
            var routeValueDictionary =
                new RouteValueDictionary(new Dictionary<string, string>
                {
                    [SmsGatewayAccessHandler.RouteValueName] = "1"
                });
            _httpRequestMock.RouteValues.Returns(routeValueDictionary);

            const int nationalSocietyId = 1;
            var gatewaySetting = new GatewaySetting { Id = 1, NationalSocietyId = nationalSocietyId };
            _nyssContextMock.GatewaySettings.FindAsync(1).Returns(gatewaySetting);

            var claims = new [] { new Claim(ClaimType.ResourceAccess, $"{SmsGatewayAccessHandler.ResourceType}:{nationalSocietyId}") };
            _userMock.Claims.Returns(claims);

            // Act
            await _smsGatewayAuthorizationHandler.HandleAsync(_authorizationHandlerContext);

            // Assert
            _authorizationHandlerContext.HasSucceeded.ShouldBe(true);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("notANumber")]
        public async Task SmsGatewayAccessHandler_WhenRouteValueIsWrong_ShouldNotSucceed(string smsGatewayId)
        {
            // Arrange
            var routeValueDictionary =
                new RouteValueDictionary(new Dictionary<string, string>
                {
                    [SmsGatewayAccessHandler.RouteValueName] = smsGatewayId
                });
            _httpRequestMock.RouteValues.Returns(routeValueDictionary);

            // Act
            await _smsGatewayAuthorizationHandler.HandleAsync(_authorizationHandlerContext);

            // Assert
            _authorizationHandlerContext.HasSucceeded.ShouldBe(false);
        }

        [Fact]
        public async Task SmsGatewayAccessHandler_WhenUserHasNotAccessToSmsGateway_ShouldNotSucceed()
        {
            // Arrange
            var routeValueDictionary =
                new RouteValueDictionary(new Dictionary<string, string>
                {
                    [SmsGatewayAccessHandler.RouteValueName] = "1"
                });
            _httpRequestMock.RouteValues.Returns(routeValueDictionary);

            var gatewaySetting = new GatewaySetting { Id = 1, NationalSocietyId = 1 };
            _nyssContextMock.GatewaySettings.FindAsync(1).Returns(gatewaySetting);

            _userMock.Claims.Returns(Enumerable.Empty<Claim>());

            // Act
            await _smsGatewayAuthorizationHandler.HandleAsync(_authorizationHandlerContext);

            // Assert
            _authorizationHandlerContext.HasSucceeded.ShouldBe(false);
        }
    }
}
