namespace TaskFlow.Application.Abstractions.Authentication;

public interface IJwtTokenProvider
{
    string GenerateToken(Guid userId, string email);
}