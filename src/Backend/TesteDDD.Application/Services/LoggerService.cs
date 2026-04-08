using Serilog;
using Serilog.Context;

namespace TesteDDD.Application.Services;

/// <summary>
/// Serviço para logging estruturado com Serilog
/// </summary>
public static class LoggerService
{
    public static void LogInfo(string message, params object[] args)
    {
        Log.Information(message, args);
    }

    public static void LogWarning(string message, params object[] args)
    {
        Log.Warning(message, args);
    }

    public static void LogError(Exception ex, string message, params object[] args)
    {
        Log.Error(ex, message, args);
    }

    public static void LogDebug(string message, params object[] args)
    {
        Log.Debug(message, args);
    }

    /// <summary>
    /// Registra contexto para correlação de logs
    /// </summary>
    public static IDisposable BeginScope(string key, object value)
    {
        return LogContext.PushProperty(key, value);
    }
}
