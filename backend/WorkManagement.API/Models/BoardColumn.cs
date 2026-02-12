using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkManagement.API.Models;

public class BoardColumn : BaseEntity
{
    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;

    public int StatusId { get; set; }
    public WorkflowStatus Status { get; set; } = null!;

    public int Order { get; set; }
}
