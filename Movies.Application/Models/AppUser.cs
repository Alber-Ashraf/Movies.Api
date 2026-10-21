namespace Movies.Application.Models
{
    public class AppUser
    {
        public required Guid Id { get; init; }
        public required string Email { get; init; }
        public required string FullName { get; init; }
        public required string PasswordHash { get; init; }
        public required string Role { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }

    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string TrustedMember = "TrustedMember";
        public const string Member = "Member";
    }
}
