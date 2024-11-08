namespace FinabitEmployee.Data
{
    public class SendMessageDto
    {
        public string Content { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; } // For private chat
        //public bool IsGroupChat { get; set; }
        //public int? GroupId { get; set; }
    }
}
