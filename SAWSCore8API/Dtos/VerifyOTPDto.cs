namespace SAWSCore8API.Dtos
{
    public class VerifyOTPDto
    {
        public required string Email { get; set; }
        public required string OTP { get; set; }
        public required string NewPassword { get; set; }
    }
}