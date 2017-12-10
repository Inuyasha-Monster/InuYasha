using System.ComponentModel.DataAnnotations;

namespace InuYasha.Service
{
    public interface ITestCheckInput
    {
        void Register(RegisterInput input);
    }

    public class RegisterInput
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(18, MinimumLength = 6)]
        public string Password { get; set; }
    }
}