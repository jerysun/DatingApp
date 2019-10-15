namespace DatingApp.API.Helpers
{
    public class MessageParams
    {
        private const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1; // initial value, it's 1-based index
        private int pageSize = 10; // initial value
        public int PageSize {
            get { return pageSize; }
            set { pageSize = value > MaxPageSize ? MaxPageSize : value; }
        }

        public int UserId { get; set; } // Sender or Recipient Id used to filter the messages
        public string MessageContainer { get; set; } = "Unread";// It's for message received
    }
}