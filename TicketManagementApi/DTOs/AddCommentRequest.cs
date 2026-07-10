using System.ComponentModel.DataAnnotations;
using TicketManagementApi.Enums;

namespace TicketManagementApi.DTOs;

public class AddCommentRequest
{
    public CommentAuthorRole AuthorRole { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
}
