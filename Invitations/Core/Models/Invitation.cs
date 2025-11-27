namespace Core.Models;

public class Invitation
{
    public int Id { get; set; }
    // This is foreign key to User.Id
    public string SenderId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string ReceiverRole { get; set; } = default!;
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive => DateTime.Now <= CreatedAt.AddDays(7);
}
