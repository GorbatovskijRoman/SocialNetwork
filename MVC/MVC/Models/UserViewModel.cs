using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace MVC.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class RoleEditModel
    {
        public AppRole Role { get; set; }
        public IEnumerable<AppUser> Members { get; set; }
        public IEnumerable<AppUser> NonMembers { get; set; }
    }

    public class RoleModificationModel
    {
        [Required]
        public string RoleName { get; set; }
        public string[] IdsToAdd { get; set; }
        public string[] IdsToDelete { get; set; }
    }

    public class Subscribe
    {
        [Required]
        public int SubscribeId { get; set; }
        [Required]
        public string OwnerId { get; set; }
        [Required]
        public virtual AppUser AppUsers { get; set; }
    }

    public class Block
    {
        [Required]
        public int BlockId { get; set; }
        [Required]
        public string OwnerId { get; set; }
        [Required]
        public virtual AppUser AppUsers { get; set; }
    }

}