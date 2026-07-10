using TicketManagementApi.Enums;

namespace TicketManagementApi.DTOs;

public class TicketCommentResponse
{
    public int Id { get; set; }
    public CommentAuthorRole AuthorRole { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
