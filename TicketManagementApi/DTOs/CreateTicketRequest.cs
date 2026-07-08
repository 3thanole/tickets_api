using System.ComponentModel.DataAnnotations;
using TicketManagementApi.Enums;

namespace TicketManagementApi.DTOs;

public class CreateTicketRequest
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public TicketPriority Priority { get; set; } = TicketPriority.Standard;
}