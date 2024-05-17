namespace MagicVilla_VillaAPI.Models.DTO
{
    public class LoginResponseDto
    {
        public LocalUser User { get; set; }
        public string Token {  get; set; }
    }
}
