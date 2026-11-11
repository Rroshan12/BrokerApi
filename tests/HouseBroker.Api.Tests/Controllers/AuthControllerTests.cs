using FluentAssertions;
using HouseBroker.Api.Controllers;
using HouseBroker.Application.Auth;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HouseBroker.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _service = new();
    private readonly AuthController _sut;
    private static readonly AuthResponse Response = new("jwt", DateTime.UtcNow.AddHours(1), "u1", "a@x.com", "Asha", "Broker");

    public AuthControllerTests() => _sut = new AuthController(_service.Object);

    [Fact]
    public async Task RegisterAsync_ReturnsOkWithToken()
    {
        var request = new RegisterRequest("a@x.com", "Passw0rd!", "Asha", AccountType.Broker, "+977 9800000000", null);
        _service.Setup(s => s.RegisterAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(Response);

        var result = await _sut.RegisterAsync(request, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(Response);
    }

    [Fact]
    public async Task LoginAsync_ReturnsOkWithToken()
    {
        var request = new LoginRequest("a@x.com", "Passw0rd!");
        _service.Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(Response);

        var result = await _sut.LoginAsync(request, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(Response);
    }
}
