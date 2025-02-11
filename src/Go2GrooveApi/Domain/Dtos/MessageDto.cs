using Go2GrooveApi.Domain.Models;

namespace Go2GrooveApi.Domain.Dtos
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
