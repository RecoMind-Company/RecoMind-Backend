namespace Core.DTOs.PlanDtos.Plan;

public class ListOfPlansDto
{
    public IEnumerable<ShortPlanDto> shortPlanDtos { get; set; } = [];
}
