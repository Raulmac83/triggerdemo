namespace triggers.repo.Auth;

public record LoginRequest(string Username, string Password);

public record AuthenticatedUser(int Id, string Username, string Email, IReadOnlyList<string> Roles);

public record LoginResult(string AccessToken, DateTime ExpiresAt, AuthenticatedUser User);
