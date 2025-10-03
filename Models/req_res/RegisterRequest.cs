namespace gameshop_api.Models.req_res
{
    public record RegisterRequest(
        string Email,
        string Password,
        string Fullname,
        string? Phone
    );
}