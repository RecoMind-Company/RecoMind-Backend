namespace Core.Models;

public class Invitation
{
    public int Id { get; set; }
    // This is foreign key to User.Id
    public string SenderId { get; set; } = default!;
    public string CompanyId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string ReceiverRole { get; set; } = default!;
    public Status Status { get; private set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive => DateTime.Now <= CreatedAt.AddDays(7);

    public Status TryToAcceptInvitation()
    {
        if (!IsActive && Status != Status.Accepted)
            Status = Status.Expired;
        else if (Status == Status.Pending)
            Status = Status.Accepted;
        return Status;
    }
}
