public class ErrorToReturn
{
    public int StatusCode { get; set; }
    public string ErrorMessage { get; set; } = default!;
    public object? Errors { get; set; }
}