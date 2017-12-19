using System.ComponentModel.DataAnnotations;

namespace TooYoung.Web.ViewModels
{
    public class LoginModel
    {
        [StringLength(20, MinimumLength = 5, ErrorMessage = "用户名必须为5~20个字符"), Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
