namespace Authentication.Core.Constants;

public static class Roles
{
    public const string Admin = "admin";
    public const string Employee = "employee";
    public const string TeamLeader = "teamleader";
    public const string Manager = "manager";
    public static List<string> AllRoles = [Admin, Employee, TeamLeader, Manager];
}
