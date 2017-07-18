using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace MVC.Models
{
    public class ChatContent
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class PasswordChangeModel
    {
        [Required]
        public string OldPass { get; set; }
        [Required]
        public string NewPass { get; set; }
    }

    public class MessageModel
    {
        [Key]
        public int MessageId { get; set; }

        public AppUser RecieverId { get; set; }
        public AppUser SenderId { get; set; }

        public DateTime SendTime { get; set; }
        public string MessageText { get; set; }
    }

    public class ConnectionInfo
    {
        [Key]
        [ForeignKey("AppUserOf")]
        public string AppUserId { get; set; }
        public string ConnectionId { get; set; }
        public bool StatusConnection { get; set; }

        public virtual AppUser AppUserOf { get; set; }
    }

    public class Subscribe
    {
        [Key]
        [ForeignKey("AppUserOf")]
        public string AppUserId { get; set; }
        public virtual List<AppUser> UserSubscribers { get; set; }

        public virtual AppUser AppUserOf { get; set; }
    }

    public class Block
    {
        [Key]
        [ForeignKey("AppUserOf")]
        public string AppUserId { get; set; }
        public virtual List<AppUser> UserBlocks { get; set; }

        public virtual AppUser AppUserOf { get; set; }
    }

}