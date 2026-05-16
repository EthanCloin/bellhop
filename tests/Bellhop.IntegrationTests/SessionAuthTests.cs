using System.Net;
using System.Net.Http.Json;
using Bellhop.Features.Auth.Session;
using Bellhop.Features.Users;
using Xunit;

namespace Bellhop.IntegrationTests;

public class SessionAuthTests : IClassFixture<BellhopWebApplicationFactory>
{
    private readonly BellhopWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;

    public SessionAuthTests(BellhopWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Full_Session_Auth_Flow_Succeeds()
    {
        // 1. Register User
        var registerRequest = new RegisterEndpoint.RegisterRequest("testuser", "P@ssword123");
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        // 2. Login
        var loginRequest = new SessionEndpoints.LoginRequest("testuser", "P@ssword123");
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/session/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        // 3. Hit /me endpoint
        var meResponse = await _client.GetAsync("/api/v1/auth/session/me");
        meResponse.EnsureSuccessStatusCode();
        
        var user = await meResponse.Content.ReadFromJsonAsync<SessionEndpoints.UserResponse>();
        Assert.NotNull(user);
        Assert.Equal("testuser", user.Username);
        Assert.NotNull(user.SessionToken);

        // 4. Logout
        var logoutResponse = await _client.PostAsync("/api/v1/auth/session/logout", null);
        logoutResponse.EnsureSuccessStatusCode();

        // Verify /me now returns Unauthorized
        var meAfterLogout = await _client.GetAsync("/api/v1/auth/session/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meAfterLogout.StatusCode);
    }
}
