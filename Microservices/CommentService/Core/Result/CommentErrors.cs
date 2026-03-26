namespace Core.Result;

internal static class CommentErrors
{
    internal static Error NotFound => new("Comment.NotFound", "Comment isn't found");
    internal static Error AccessDenied => new("Comment.AccessDenied", "You aren't allowed to perform this action on the comment");
    internal static Error EditTimeout => new("Comment.EditTimeout", "you can't edit this comment now");
}
