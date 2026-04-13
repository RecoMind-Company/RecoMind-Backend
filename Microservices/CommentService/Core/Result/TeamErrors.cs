namespace Core.Result;

public static class TeamErrors
{
    internal static Error UserNotInTeam => new("Comment.UserNotInTeam", "User isn't in the team");

}
