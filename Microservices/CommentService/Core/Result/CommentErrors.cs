namespace Core.Result;

public static class CommentErrors
{
    public static Error NotFound => new("Comment.NotFound", "Comment isn't found");
    public static Error AccessDenied => new("Comment.AccessDenied", "You aren't allowed to perform this action on the comment");
    public static Error EditTimeout => new("Comment.EditTimeout", "you can't edit this comment now");
}
