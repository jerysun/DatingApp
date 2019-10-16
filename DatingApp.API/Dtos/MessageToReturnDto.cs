using System;

namespace DatingApp.API.Dtos
{
    public class MessageToReturnDto
    {
        //AutoMapper is smart enough, if it finds the SenderId(Id in User.cs),
        //it will do its best to automatically map KnownAs to SenderKnownAs,
        //as well as the PhotoUrl of the Main photo from Photos in User.cs to
        //SenderPhotoUrl. The same for Recipient equivalents.
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderKnownAs { get; set; }
        public string SenderPhotoUrl { get; set; }
        public int RecipientId { get; set; }
        public string RecipientKnownAs { get; set; }
        public string RecipientPhotoUrl { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; } // It should be null if it isn't read yet
        public DateTime MessageSent { get; set; }
    }
}