using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CommentTranslator22.Translate
{

    /// <summary>
    /// 任务执行器，支持超时控制和并发任务限制。
    /// </summary>
    /// <summary>
    /// 任务执行器，支持超时控制和并发任务限制。
    /// </summary>
    public class TaskExecutor
    {
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// 初始化任务执行器。
        /// </summary>
        public TaskExecutor() : this(0)
        {
        }

        /// <summary>
        /// 初始化任务执行器。
        /// </summary>
        /// <param name="maxConcurrentTasks">最大并发任务数。如果为 0，则不限制并发数。</param>
        public TaskExecutor(int maxConcurrentTasks)
        {
            if (maxConcurrentTasks < 0)
                throw new ArgumentOutOfRangeException(nameof(maxConcurrentTasks), "最大并发任务数不能为负数。");

            // 如果 maxConcurrentTasks 为 0，则不使用信号量
            _semaphore = maxConcurrentTasks > 0 ? new SemaphoreSlim(maxConcurrentTasks) : null;
        }

        /// <summary>
        /// 执行一个异步任务，并在指定时间内等待结果。根据参数决定是否在超时后中断任务。
        /// </summary>
        /// <typeparam name="TResult">任务的返回类型。</typeparam>
        /// <param name="taskFunc">要执行的异步任务。</param>
        /// <param name="timeout">超时时间。</param>
        /// <param name="cancelOnTimeout">是否在超时后中断任务。</param>
        /// <returns>任务的结果。如果超时且不中断任务，则返回 null。</returns>
        public async Task<TResult> RunWithTimeoutAsync<TResult>(
            Func<Task<TResult>> taskFunc,
            TimeSpan timeout,
            bool cancelOnTimeout = false)
        {
            // 如果信号量存在，则等待信号量
            if (_semaphore != null)
                await _semaphore.WaitAsync();

            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    // 启动任务
                    Task<TResult> task = taskFunc();

                    // 使用 Task.WhenAny 等待任务完成或超时
                    Task delayTask = Task.Delay(timeout, cts.Token);
                    Task completedTask = await Task.WhenAny(task, delayTask);

                    if (completedTask == task)
                    {
                        // 任务在超时前完成
                        cts.Cancel(); // 取消 Delay 任务
                        return await task; // 返回任务结果
                    }
                    else
                    {
                        // 任务超时
                        if (cancelOnTimeout)
                        {
                            cts.Cancel(); // 取消任务
                            throw new TimeoutException("任务超时，已取消");
                        }
                        else
                        {
                            // 不取消任务，返回 null
                            return default;
                        }
                    }
                }
            }
            finally
            {
                // 如果信号量存在，则释放信号量
                _semaphore?.Release();
            }
        }

        /// <summary>
        /// 静态方法：执行一个异步任务，并在指定时间内等待结果。
        /// </summary>
        /// <typeparam name="TResult">任务的返回类型。</typeparam>
        /// <param name="taskFunc">要执行的异步任务。</param>
        /// <param name="timeout">超时时间。</param>
        /// <param name="cancelOnTimeout">是否在超时后中断任务。</param>
        /// <param name="maxConcurrentTasks">最大并发任务数。如果为 0，则不限制并发数。</param>
        /// <returns>任务的结果。如果超时且不中断任务，则返回 null。</returns>
        public static async Task<TResult> RunWithTimeoutAsync<TResult>(
            Func<Task<TResult>> taskFunc,
            TimeSpan timeout,
            bool cancelOnTimeout = false,
            int maxConcurrentTasks = 0)
        {
            // 创建临时 TaskExecutor 实例
            var executor = new TaskExecutor(maxConcurrentTasks);
            return await executor.RunWithTimeoutAsync(taskFunc, timeout, cancelOnTimeout);
        }

        static async Task TaskAsync()
        {
            // 创建任务执行器，设置最大并发任务数为 2
            var taskExecutor = new TaskExecutor(maxConcurrentTasks: 2);

            // 创建任务列表
            var tasks = new List<Task<bool>>();
            for (int i = 1; i <= 5; i++)
            {
                tasks.Add(taskExecutor.RunWithTimeoutAsync(
                    () => LongRunningMethodAsync($"任务{i}"),
                    TimeSpan.FromSeconds(3),
                    cancelOnTimeout: false
                ));
            }

            // 等待所有任务完成
            var results = await Task.WhenAll(tasks);

            // 输出结果
            for (int i = 0; i < results.Length; i++)
            {
                Console.WriteLine(results[i] ? $"任务{i + 1} 完成，结果为: {results[i]}" : $"任务{i + 1} 超时，跳过结果");
            }
        }

        /// <summary>
        /// 模拟一个长时间运行的异步任务
        /// </summary>
        static async Task<bool> LongRunningMethodAsync(string taskName)
        {
            Console.WriteLine($"{taskName} 开始执行");
            await Task.Delay(5000); // 模拟耗时操作
            Console.WriteLine($"{taskName} 执行完成");
            return true; // 返回结果
        }
    }
}