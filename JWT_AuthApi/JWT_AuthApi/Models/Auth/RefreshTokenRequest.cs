using System.ComponentModel.DataAnnotations;

namespace JWT_AuthApi.Models.Auth
{
    public class RefreshTokenRequest
    {
        [Required]
        public string JwtToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
