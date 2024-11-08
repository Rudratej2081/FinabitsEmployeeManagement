using FinabitEmployee.Models;
using System;
using System.Text.Json.Serialization; // For System.Text.Json
// using Newtonsoft.Json; // Uncomment if using Newtonsoft.Json

namespace FinabitEmployee.Data
{
    public class Message
    {
        public int Id { get; set; }

        // Ensure content is not null or too long (add a property validation if needed)
        public string Content { get; set; } = string.Empty; // Initialize to avoid null issues

        public DateTime SentAt { get; set; } = DateTime.UtcNow; // Default value

        public string SenderId { get; set; } = string.Empty; // Initialize

        public string ReceiverId { get; set; } = string.Empty; // Initialize

        // If there's a circular reference, consider ignoring this property
        [JsonIgnore] // Use JsonIgnore if using System.Text.Json
        // [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)] // Use for Newtonsoft.Json
        public ApplicationUser Sender { get; set; }
    }
}
