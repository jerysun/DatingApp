using System;

namespace DatingApp.API.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public virtual User Sender { get; set; }
        public int RecipientId { get; set; }
        public virtual User Recipient { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; } // It should be null if it isn't read yet
        public DateTime MessageSent { get; set; }
        public bool SenderDeleted { get; set; }
        // We won't physically delete the message unless both parties deleted the message
        public bool RecipientDeleted { get; set; }
    }
}