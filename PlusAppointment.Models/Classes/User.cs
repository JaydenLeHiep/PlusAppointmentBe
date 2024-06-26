using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using PlusAppointment.Models.Enums;
using PlusAppointment.Models.Interfaces;

namespace PlusAppointment.Models.Classes
{
    public class User : IUserIdentity
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Store hashed passwords
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Role Role { get; set; }

        [JsonConverter(typeof(BusinessCollectionConverter))]
        public ICollection<Business> Businesses { get; set; } = new List<Business>();

        public User() { } // Parameterless constructor for deserialization

        public User(string username, string password, string email, string phone, Role role)
        {
            Username = username;
            Password = password;
            Email = email;
            Phone = phone;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Role = role;
        }

        int IUserIdentity.Id => UserId;
        string IUserIdentity.Username => Username;
        string IUserIdentity.Role => Role.ToString();
    }

    public class BusinessCollectionConverter : JsonConverter<ICollection<Business>>
    {
        public override ICollection<Business> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var businesses = JsonSerializer.Deserialize<List<Business>>(ref reader, options);
            return businesses ?? new List<Business>();
        }

        public override void Write(Utf8JsonWriter writer, ICollection<Business> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}