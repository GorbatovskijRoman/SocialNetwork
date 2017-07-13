using System;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MVC.Models
{
    public class AppUser : IdentityUser
    { 
        public string NewPass { get; set; }

        public string OldPass { get; set; }

        public byte[] Avatar { get; set; }

        public bool Admin { get; set; }

        public virtual List<Subscribe> Subscribes { get; set; }

        public virtual List<Block> Blocks { get; set; }
    }
}