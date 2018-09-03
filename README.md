# DelayEventLoop
一个简单的事件循环,用来处理大量的简单延时任务,避免使用大量timer导致性能低下
比如网络游戏中的技能冷却计算等

示例:
```
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
```
