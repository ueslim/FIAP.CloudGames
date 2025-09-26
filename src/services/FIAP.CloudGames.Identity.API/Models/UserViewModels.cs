using System.ComponentModel.DataAnnotations;

namespace FIAP.CloudGames.Identity.API.Models
{
    public class UserViewModels
    {
        public class UserRegistration
        {
            [Required(ErrorMessage = "Field {0} is required")]
            public string Name { get; set; }

            [Required(ErrorMessage = "Field {0} is required")]
            public string Cpf { get; set; }

            [Required(ErrorMessage = "Field {0} is required")]
            [EmailAddress(ErrorMessage = "Field {0} is in an invalid format")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Field {0} is required")]
            [StringLength(100, ErrorMessage = "The {0} field must be between {2} and {1} characters", MinimumLength = 6)]
            public string Password { get; set; }

            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            public string PasswordConfirmation { get; set; }
        }

        public class UserLogin
        {
            [Required(ErrorMessage = "Field {0} is required")]
            [EmailAddress(ErrorMessage = "Field {0} is in an invalid format")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Field {0} is required")]
            [StringLength(100, ErrorMessage = "The {0} field must be between {2} and {1} characters", MinimumLength = 6)]
            public string Password { get; set; }
        }

        public class UserResponseLogin
        {
            public string AccessToken { get; set; }
            public Guid RefreshToken { get; set; }
            public double ExpiresIn { get; set; }
            public UserToken UserToken { get; set; }
        }

        public class UserToken
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public IEnumerable<UserClaim> Claims { get; set; }
        }

        public class UserClaim
        {
            public string Value { get; set; }
            public string Type { get; set; }
        }
    }
}