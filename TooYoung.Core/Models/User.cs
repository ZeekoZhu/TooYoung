using System.Collections.Generic;
using TooYoung.Core.Permissions;

namespace TooYoung.Core.Models
{
    public class User
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public Permission Permission { get; set; }

        public List<Group> Groups { get; set; } = new List<Group>();
    }
}
