namespace triggers.repo.Auth;

public interface IAuthService
{
    Task<LoginResult?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthenticatedUser?> GetByIdAsync(int userId, CancellationToken ct = default);
}
