using System.Collections.Concurrent;
using TGA.CoreLib.Bot.Messages.Interfaces;

namespace TGA.CoreLib.Bot.Messages
{
    public class MessagesSender : ISender
    {
        private readonly ConcurrentQueue<ISendedItem> _sendingQueue = new();
        private readonly Task _sendingTask;
        private readonly int _sendingBatchSize = 30;
        private readonly TimeSpan _sendingPeriod = new(0, 0, 1);
        public MessagesSender()
        {
            _sendingTask = Task.Factory.StartNew(Executor, CancellationToken.None, TaskCreationOptions.LongRunning);
        }

        public void Add(ISendedItem sendedItem)
        {
            _sendingQueue.Enqueue(sendedItem);
        }

        private async Task Executor(object? token)
        {
            if (token is CancellationToken _token)
            {
                Dictionary<long, Task>? mappedTasks = new();
                Queue<ISendedItem>? sendingBuffer = new();
                List<ISendedItem>? sendingBufferTemp = new();

                List<Task>? tasks = new();
                while (!_token.IsCancellationRequested)
                {
                    var keys = mappedTasks.Keys;
                    tasks.RemoveAll(item => item.IsCompleted);
                    foreach (long key in keys)
                    {
                        if (mappedTasks[key].IsCompleted)
                        {
                            mappedTasks.Remove(key);
                        }
                        else
                        {
                            tasks.Add(mappedTasks[key]);
                        }
                    }

                    while (mappedTasks.Count < 30 && sendingBuffer.TryDequeue(out ISendedItem? item))
                    {
                        SendOrQueueMessage(mappedTasks, tasks, sendingBufferTemp, item);
                    }

                    while (mappedTasks.Count < _sendingBatchSize && _sendingQueue.TryDequeue(out ISendedItem? item))
                    {
                        SendOrQueueMessage(mappedTasks, tasks, sendingBufferTemp, item);
                    }

                    sendingBufferTemp.ForEach(item => sendingBuffer.Enqueue(item));
                    sendingBufferTemp.Clear();

                    await Task.WhenAll(Task.Delay(_sendingPeriod), tasks.Count > 0 ? Task.WhenAny(tasks) : Task.CompletedTask);
                    if (sendingBuffer.Count > 1000000)
                    {
                        sendingBuffer.Clear();
                    }
                }
            }
        }

        private static void SendOrQueueMessage(Dictionary<long, Task> mappedTasks,
            List<Task> tasks,
            List<ISendedItem> sendingBuffer,
            ISendedItem sendedItem)
        {
            if (!mappedTasks.ContainsKey(sendedItem.ChatId))
            {
                Task<global::Telegram.Bot.Types.Message>? sendingTask = sendedItem.Send();
                mappedTasks.Add(sendedItem.ChatId, sendingTask);
                tasks.Add(sendingTask);
            }
            else
            {
                sendingBuffer.Add(sendedItem);
            }
        }
    }
}
