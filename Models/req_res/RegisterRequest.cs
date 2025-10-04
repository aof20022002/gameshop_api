namespace gameshop_api.Models.req_res
{
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
    }
}