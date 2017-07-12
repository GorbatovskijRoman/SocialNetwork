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

    public class SubscribesViewModel
    {
        public string AccountOwnerId { get; set; }
        public string SubscriberId { get; set; }
    }

    public class BlackListViewModel
    {
        public string AccountOwnerId { get; set; }
        public string BlockedId { get; set; }
    }

}