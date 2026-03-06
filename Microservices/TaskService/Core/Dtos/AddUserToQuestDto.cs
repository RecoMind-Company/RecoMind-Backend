using System.ComponentModel.DataAnnotations;

namespace Core.Dtos;

public class AddUserToQuestDto
{
    [Required(ErrorMessage = "UserId is required.")]
    public string UserId { get; set; } = default!;
    [Required(ErrorMessage = "QuestId is required.")]
    public string QuestId { get; set; } = default!;
    [Required(ErrorMessage = "TeamId is required.")]
    public string TeamId { get; set; } = default!;
}
