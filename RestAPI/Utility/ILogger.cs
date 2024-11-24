namespace RestAPI.Utility;

public interface ILogger
{
    public void Debug(object? message);
    public void Info(object? message);
    public void Error(object? message);
    public void Warning(object? message);
}