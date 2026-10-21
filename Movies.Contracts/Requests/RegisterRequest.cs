namespace Movies.Contracts.Requests
{
    public class RegisterRequest
    {
        public required string Email { get; init; }
        public required string FullName { get; init; }
        public required string Password { get; init; }
    }
}
