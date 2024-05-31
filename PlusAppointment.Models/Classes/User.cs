using PlusAppointment.Models.Enums;
using PlusAppointment.Models.Interfaces;
using System;
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class User : IUserIdentity
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Store hashed passwords
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Role Role { get; set; }
        public ICollection<Business> Businesses { get; set; } = new List<Business>();

        int IUserIdentity.Id => UserId;
        string IUserIdentity.Username => Username;
        string IUserIdentity.Role => Role.ToString();
    }
}