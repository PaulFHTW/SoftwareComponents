using System.Reflection;
using log4net;
using log4net.Config;

namespace RestAPI.Utility;

public class Logger : ILogger
{
    private readonly ILog _log = LogManager.GetLogger(typeof(Logger));
    
    public Logger()
    {
        var logRepo = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepo, new FileInfo("log4net.config"));
    }
    
    public void Debug(object? message)
    {
        _log.Debug(message);
    }

    public void Info(object? message)
    {
        _log.Info(message);
    }

    public void Error(object? message)
    {
        _log.Error(message);
    }

    public void Warning(object? message)
    {
        _log.Warn(message);
    }
}