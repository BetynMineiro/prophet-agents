namespace Prophet.Application.Interfaces;

/// <summary>
/// Provides the identity of the currently authenticated user.
/// </summary>
public interface ICurrentUserContext
{
    Guid UserId { get; }
}
