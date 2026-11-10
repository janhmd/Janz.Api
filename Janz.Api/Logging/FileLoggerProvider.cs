using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Janz.Api.Logging
{
    // Əlavə paket olmadan fayla loglama: hər gün üçün ayrıca fayl, köhnələr silinir.
    // Yazma ayrıca thread-dədir: sorğu fayla yazılmasını gözləmir. Səviyyələr appsettings-in "Logging" bölməsindən gəlir
    // (provayderin adı "File": Logging:File:LogLevel ilə ayrıca dəyişmək olar)
    [ProviderAlias("File")]
    public sealed class FileLoggerProvider : ILoggerProvider
    {
        private readonly FileLoggerOptions _options;
        private readonly string _directory;
        private readonly BlockingCollection<LogEntry> _queue = new(new ConcurrentQueue<LogEntry>(), 10_000);
        private readonly Thread _worker;
        private static readonly Encoding Utf8 = new UTF8Encoding(false);
        private DateOnly _cleanedOn;

        public FileLoggerProvider(IOptions<FileLoggerOptions> options, IHostEnvironment environment)
        {
            _options = options.Value;
            _directory = System.IO.Path.IsPathRooted(_options.Path)
                ? _options.Path
                : System.IO.Path.Combine(environment.ContentRootPath, _options.Path);

            Directory.CreateDirectory(_directory);

            _worker = new Thread(Run) { IsBackground = true, Name = "Janz file logger" };
            _worker.Start();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, Enqueue);
        }

        private void Enqueue(LogEntry entry)
        {
            // Növbə dolubsa (disk çox yavaşdır) yeni sətir atılır: loglama heç vaxt sorğunu dayandırmamalıdır
            _queue.TryAdd(entry);
        }

        private void Run()
        {
            foreach (var entry in _queue.GetConsumingEnumerable())
            {
                try
                {
                    var line = Format(entry);
                    var day = entry.Time.ToString("yyyyMMdd");

                    File.AppendAllText(System.IO.Path.Combine(_directory, $"janz-{day}.log"), line, Utf8);

                    if (_options.SeparateErrorFile && entry.Level >= LogLevel.Warning)
                    {
                        File.AppendAllText(System.IO.Path.Combine(_directory, $"errors-{day}.log"), line, Utf8);
                    }

                    DeleteOldFiles(DateOnly.FromDateTime(entry.Time.LocalDateTime));
                }
                catch (Exception)
                {
                    // Fayl bağlıdırsa və ya disk doludursa loglama tətbiqi yıxmamalıdır
                }
            }
        }

        private void DeleteOldFiles(DateOnly today)
        {
            if (_cleanedOn == today)
            {
                return;
            }
            _cleanedOn = today;

            var limit = DateTime.Now.AddDays(-Math.Max(_options.RetainDays, 1));
            foreach (var file in Directory.EnumerateFiles(_directory, "*.log"))
            {
                if (File.GetLastWriteTime(file) < limit)
                {
                    File.Delete(file);
                }
            }
        }

        private static string Format(LogEntry entry)
        {
            var level = entry.Level switch
            {
                LogLevel.Trace => "TRC",
                LogLevel.Debug => "DBG",
                LogLevel.Information => "INF",
                LogLevel.Warning => "WRN",
                LogLevel.Error => "ERR",
                LogLevel.Critical => "CRT",
                _ => "???"
            };

            var text = new StringBuilder();
            text.Append(entry.Time.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"))
                .Append(" [").Append(level).Append("] ")
                .Append(entry.Category).Append(": ")
                .Append(entry.Message.ReplaceLineEndings(" "))
                .AppendLine();

            if (entry.Exception is not null)
            {
                text.AppendLine(entry.Exception.ToString());
            }

            return text.ToString();
        }

        public void Dispose()
        {
            // Növbədə qalanlar yazılsın, sonra thread bitsin
            _queue.CompleteAdding();
            _worker.Join(TimeSpan.FromSeconds(3));
            _queue.Dispose();
        }

        internal sealed record LogEntry(DateTimeOffset Time, LogLevel Level, string Category, string Message, Exception? Exception);

        private sealed class FileLogger : ILogger
        {
            private readonly string _category;
            private readonly Action<LogEntry> _write;

            public FileLogger(string category, Action<LogEntry> write)
            {
                _category = category;
                _write = write;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            // Səviyyə filtrini Logging bölməsi özü edir
            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                var message = formatter(state, exception);
                _write(new LogEntry(DateTimeOffset.Now, logLevel, _category, message, exception));
            }
        }
    }
}
