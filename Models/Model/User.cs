using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Models.Model
{
    public class User: IdentityUser
    {
        [Required]
        public string Name { get; set; }
    }
}
