using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkManagement.API.Models;

public class Board : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    // If null, it's a shared/default board. If set, it's a personal board.
    public int? OwnerId { get; set; }
    public User? Owner { get; set; }

    public bool IsDefault { get; set; } = false;

    public ICollection<BoardColumn> Columns { get; set; } = new List<BoardColumn>();
}
