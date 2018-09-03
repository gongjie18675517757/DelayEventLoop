using System;
using System.Threading.Tasks;

namespace DelayEventLoop.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            for (int i = 0; i < 100; i++)
            {
                var index = i;
                Task.Run(async () =>
                {
                    var delayTask= DelayTask.AddTask(index,
                        new Func<object, int, Func<Task>>((state, count) => new Func<Task>(() =>
                        {
                            Console.WriteLine($"第{count}次执行:{state},threadId:{System.Threading.Thread.CurrentThread.ManagedThreadId}");
                            return Task.CompletedTask;
                        })),
                        index,
                        TimeSpan.FromSeconds(1),
                        count =>
                        {
                            if (count > i)
                                return null;
                            return TimeSpan.FromMilliseconds((i % 3) * 100);
                        });
                    await Task.Delay(5000);
                    delayTask.Stop();
                });
            }

            Console.ReadLine(); 
        }
    }
}
