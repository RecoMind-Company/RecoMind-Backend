namespace Core.Result;

public static class TeamErrors
{
    public static Error UserNotInTeam => new("Comment.UserNotInTeam", "User isn't in the team");

}
