using System;

namespace NOX_Backend.Models
{
    /// <summary>
    /// Represents a ChatMessage entity in a conversation.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Unique identifier for the message.
        /// </summary>
        public int MessageId { get; set; }

        /// <summary>
        /// Foreign key reference to the Conversation this message belongs to.
        /// </summary>
        public int ConvoId { get; set; }

        /// <summary>
        /// Navigation property for the associated Conversation.
        /// </summary>
        public Conversation? Conversation { get; set; }

        /// <summary>
        /// Sender of the message ('User' or 'Noxy').
        /// </summary>
        public string? Sender { get; set; }

        /// <summary>
        /// Content of the message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Timestamp when the message was sent.
        /// </summary>
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
