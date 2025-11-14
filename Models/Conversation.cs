using System;

namespace NOX_Backend.Models
{
    /// <summary>
    /// Represents a Conversation entity containing chat messages between a user and Noxy.
    /// </summary>
    public class Conversation
    {
        /// <summary>
        /// Unique identifier for the conversation.
        /// </summary>
        public int ConvoId { get; set; }

        /// <summary>
        /// Foreign key reference to the ApplicationUser who started the conversation.
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Navigation property for the associated ApplicationUser.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Timestamp when the conversation was started.
        /// </summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property for chat messages in this conversation.
        /// </summary>
        public ICollection<ChatMessage>? Messages { get; set; }
    }
}
