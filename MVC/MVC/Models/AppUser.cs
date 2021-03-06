﻿using System;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MVC.Models
{
    public class AppUser : IdentityUser
    {
        public byte[] Avatar { get; set; }

        public bool Admin { get; set; }

        public bool ResetPass { get; set; }

        public bool ReLogin { get; set; }
    }
}