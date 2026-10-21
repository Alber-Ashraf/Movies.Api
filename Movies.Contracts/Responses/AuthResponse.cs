namespace Movies.Contracts.Responses
{
    public class AuthResponse
    {
        public required string Token { get; init; }
        public required Guid UserId { get; init; }
        public required string Email { get; init; }
        public required string FullName { get; init; }
        public required string Role { get; init; }
        public required DateTime ExpiresAt { get; init; }
    }
}
