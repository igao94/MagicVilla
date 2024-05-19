using Microsoft.AspNetCore.Identity;

namespace MagicVilla_VillaAPI.Models.DTO
{
    public class LoginResponseDto
    {
        public UserDto User { get; set; }
        public string Role {  get; set; }
        public string Token {  get; set; }
    }
}
