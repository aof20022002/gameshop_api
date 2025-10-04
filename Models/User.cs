using System.ComponentModel.DataAnnotations;

namespace gameshop_api.Models
{
    public class User
    {
        [Key]
        public int uid { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        public string? fullname { get; set; }
        public string role { get; set; } = "user";

    }
}