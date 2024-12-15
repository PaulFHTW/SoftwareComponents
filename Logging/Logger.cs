using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using log4net;
using log4net.Config;

namespace Logging;

public class Logger : ILogger
{
    private readonly ILog _log = LogManager.GetLogger(typeof(Logger));

    [ExcludeFromCodeCoverage]
    public Logger()
    {
        try
        {
            var logRepo = LogManager.GetRepository(Assembly.GetEntryAssembly()!);
            XmlConfigurator.Configure(logRepo, new FileInfo("log4net.config"));
        } catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    [ExcludeFromCodeCoverage]
    public void Debug(object? message)
    {
        _log.Debug(message);
    }

    [ExcludeFromCodeCoverage]
    public void Info(object? message)
    {
        _log.Info(message);
    }

    [ExcludeFromCodeCoverage]
    public void Error(object? message)
    {
        _log.Error(message);
    }

    [ExcludeFromCodeCoverage]
    public void Warning(object? message)
    {
        _log.Warn(message);
    }
}