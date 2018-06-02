using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TooYoung.Web.ViewModels
{
    public class ImagePostModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string GroupId { get; set; }
    }
}
