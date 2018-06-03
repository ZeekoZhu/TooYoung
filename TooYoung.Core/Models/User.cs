using System.Collections.Generic;

namespace TooYoung.Core.Models
{
    public class User
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public List<Permission> Permissions { get; set; } = new List<Permission>();

        public List<Group> Groups { get; set; } = new List<Group>();
    }
}
