using System.ComponentModel.DataAnnotations;

namespace Go2GrooveApi.Domain.Dtos
{
    public class LoginDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
