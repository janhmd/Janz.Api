using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Janz.Api.Logging
{
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, IConfiguration configuration)
        {
            builder.Services.Configure<FileLoggerOptions>(configuration.GetSection("FileLogging"));
            builder.AddConfiguration();
            builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
            LoggerProviderOptions.RegisterProviderOptions<FileLoggerOptions, FileLoggerProvider>(builder.Services);
            return builder;
        }
    }
}
