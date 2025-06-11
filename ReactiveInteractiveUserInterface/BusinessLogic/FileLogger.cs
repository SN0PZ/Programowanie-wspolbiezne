using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    public class FileLogger : IDisposable
    {
        private readonly ConcurrentQueue<string> _queue = new();
        private readonly Task _loggingTask;
        private readonly CancellationTokenSource _cts = new();
        private readonly string _filePath;

        public FileLogger(string filePath)
        {
            _filePath = filePath;
            _loggingTask = Task.Run(ProcessQueue, _cts.Token);
        }

        public void Log(string message)
        {
            _queue.Enqueue($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }

        private async Task ProcessQueue()
        {
            using (var writer = new StreamWriter(_filePath, false) { AutoFlush = true })
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    while (_queue.TryDequeue(out var message))
                    {
                        await writer.WriteLineAsync(message);
                    }
                    await Task.Delay(100, _cts.Token);
                }
            }
        }


        public void Dispose()
        {
            _cts.Cancel();
            try
            {

                _loggingTask.Wait(2000);
            }
            catch (AggregateException e)
            {

                e.Handle(ex => ex is TaskCanceledException);
            }
            finally
            {
                _cts.Dispose();
            }
        }
    }
}