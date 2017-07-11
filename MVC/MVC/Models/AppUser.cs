using System;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
namespace MVC.Models
{
    public class AppUser : IdentityUser
    { 
        public string NewPass { get; set; }

        public string[] BlockUser { get; set; }

        public string[] Subscribers { get; set; }

        public byte[] Avatar { get; set; }

        public bool Admin { get; set; }
    }
}