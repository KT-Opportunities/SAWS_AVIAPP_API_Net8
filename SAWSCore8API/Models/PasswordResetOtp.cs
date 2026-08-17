namespace SAWSCore8API.Models
{
    public class PasswordResetOtp
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string OTP { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}