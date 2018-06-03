using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TooYoung.Core.Models;

namespace TooYoung.Web.ViewModels
{
    public class RegisterModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string DisplayName { get; set; }
        [Required]
        public List<Permission> Permissions { get; set; }
        [Required]
        public string Email { get; set; }
    }
}
