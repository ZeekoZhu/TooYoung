using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TooYoung.Web.ViewModels
{
    public class GroupPostModel
    {
        [Required(ErrorMessage = "请填写一个组名")]
        [StringLength(40, MinimumLength = 5, ErrorMessage = "请填写一个长度在 5 - 40 个字符之间的组名")]
        public string Name { get; set; }
        public List<string> ACL { get; set; }
    }
}
