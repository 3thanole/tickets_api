using TicketManagementApi.Enums;

namespace TicketManagementApi.Models;

public class TicketComment
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public CommentAuthorRole AuthorRole { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
