using FluentAssertions;
using FluentValidation;
using HouseBroker.Application.Auth;
using HouseBroker.Application.Common.Exceptions;
using HouseBroker.Domain.Enums;
using Moq;

namespace HouseBroker.Application.Tests.Auth;

public class AuthServiceTests
{
    private readonly Mock<IIdentityService> _identity = new();
    private readonly Mock<IJwtTokenGenerator> _tokens = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _tokens.Setup(t => t.Generate(It.IsAny<AuthenticatedUser>()))
            .Returns(new AccessToken("jwt", new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        _sut = new AuthService(_identity.Object, _tokens.Object, new RegisterRequestValidator(), new LoginRequestValidator());
    }

    [Theory]
    [InlineData(AccountType.Broker, Roles.Broker)]
    [InlineData(AccountType.Seeker, Roles.Seeker)]
    public async Task RegisterAsync_AccountType_MapsToRole(AccountType type, string expectedRole)
    {
        _identity.Setup(i => i.CreateUserAsync(It.IsAny<NewUser>(), "Passw0rd!", expectedRole, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticatedUser("u1", "a@x.com", "Asha", expectedRole));
        var request = new RegisterRequest("a@x.com", "Passw0rd!", "Asha", type, "+977 9800000000", "Agency");

        var result = await _sut.RegisterAsync(request, CancellationToken.None);

        result.Role.Should().Be(expectedRole);
        result.AccessToken.Should().Be("jwt");
    }

    [Fact]
    public async Task RegisterAsync_BrokerWithoutPhone_ThrowsValidationException()
    {
        var request = new RegisterRequest("a@x.com", "Passw0rd!", "Asha", AccountType.Broker, null, null);

        var act = () => _sut.RegisterAsync(request, CancellationToken.None);

        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber");
    }

    [Fact]
    public async Task RegisterAsync_Seeker_DropsAgencyName()
    {
        NewUser? created = null;
        _identity.Setup(i => i.CreateUserAsync(It.IsAny<NewUser>(), It.IsAny<string>(), Roles.Seeker, It.IsAny<CancellationToken>()))
            .Callback<NewUser, string, string, CancellationToken>((u, _, _, _) => created = u)
            .ReturnsAsync(new AuthenticatedUser("u1", "a@x.com", "Asha", Roles.Seeker));

        await _sut.RegisterAsync(new RegisterRequest(" a@x.com ", "Passw0rd!", "Asha", AccountType.Seeker, null, "Agency"), CancellationToken.None);

        created!.AgencyName.Should().BeNull();
        created.Email.Should().Be("a@x.com");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentials()
    {
        _identity.Setup(i => i.ValidateCredentialsAsync("a@x.com", "wrong-pass", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthenticatedUser?)null);

        var act = () => _sut.LoginAsync(new LoginRequest("a@x.com", "wrong-pass"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        _identity.Setup(i => i.ValidateCredentialsAsync("a@x.com", "Passw0rd!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticatedUser("u1", "a@x.com", "Asha", Roles.Broker));

        var result = await _sut.LoginAsync(new LoginRequest("a@x.com", "Passw0rd!"), CancellationToken.None);

        result.AccessToken.Should().Be("jwt");
        result.UserId.Should().Be("u1");
    }
}
