using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DelayEventLoop
{
    public interface IStatus
    {

    }

    public class DelayTask
    {
        static DelayTask()
        {
            delayedTasks = new ConcurrentDictionary<object, DelayTask>();
            Task.Run(new Action(Run));
        }
        private static ConcurrentDictionary<object, DelayTask> delayedTasks;

        /// <summary>
        /// 键
        /// </summary>
        public object Key { get; }

        /// <summary>
        /// 回调
        /// </summary>
        public Func<object, int, Func<Task>> Callback { get; set; }

        /// <summary>
        /// 回调参数
        /// </summary>
        public object State { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public long DelayedTime { get; private set; }

        /// <summary>
        /// 下次时间
        /// </summary>
        public Func<int, TimeSpan?> NextDelayedTime { get; }

        /// <summary>
        /// 执行次数
        /// </summary>
        public int ExecCount { get; private set; }

        private static void Run()
        {
            long ticks;
            while (true)
            {
                ticks = DateTime.Now.Ticks;
                foreach (var keyValue in delayedTasks.ToArray())
                {
                    var key = keyValue.Key;
                    var value = keyValue.Value;
                    if (value.DelayedTime <= ticks)
                    {
                        value.ExecCount += 1;
                        try
                        {
                            Task.Run(value.Callback(value.State, value.ExecCount));
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        if (value.NextDelayedTime != null)
                        {
                            var timeSpan = value.NextDelayedTime(value.ExecCount);
                            if (timeSpan != null)
                                value.DelayedTime = ticks + timeSpan.Value.Ticks;
                            else
                                delayedTasks.TryRemove(key, out value);
                        }
                        else
                            delayedTasks.TryRemove(key, out value);
                    }
                }
            }
        }

        /// <summary>
        /// 创建一个延迟事件
        /// </summary>       
        /// <param name="cb">回调</param>
        /// <param name="state">回调参数</param>
        /// <param name="delayedTime">延迟时间</param>
        /// <param name="count">循环次数</param>
        public DelayTask(object key, Func<object, int, Func<Task>> cb, object state, TimeSpan delayedTimeSpan, Func<int, TimeSpan?> nextDelayedTimeSpan)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (cb == null)
            {
                throw new ArgumentNullException(nameof(cb));
            }

            if (delayedTasks.ContainsKey(key))
                throw new Exception("键值重复");

            var time = DateTime.Now;
            Key = key;
            Callback = cb;
            State = state;
            DelayedTime = time.Add(delayedTimeSpan).Ticks;
            NextDelayedTime = nextDelayedTimeSpan;

            delayedTasks.TryAdd(key, this);
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Stop()
        {
            return delayedTasks.TryRemove(Key, out var value);
        }

        /// <summary>
        /// 创建一个延时任务
        /// </summary>
        /// <param name="cb">回调</param>
        /// <param name="state">回调参数</param>
        /// <param name="delayedTime">延迟时间</param>
        /// <param name="count">循环次数</param>
        public static DelayTask AddTask(object key, Func<object, int, Func<Task>> cb, object state, TimeSpan delayedTimeSpan, Func<int, TimeSpan?> nextDelayedTimeSpan)
        {
            return new DelayTask(key, cb, state, delayedTimeSpan, nextDelayedTimeSpan);
        }

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool RemoveTask(object key)
        {
            return delayedTasks.TryRemove(key, out var value);
        }
    }
}
