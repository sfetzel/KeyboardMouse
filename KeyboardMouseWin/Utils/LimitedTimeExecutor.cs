using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardMouseWin.Utils
{
    /// <summary>
    /// Helper class which executes a task and possible subtasks until a time limit is reached.
    /// </summary>
    public class LimitedTimeExecutor
    {
        /// <summary>
        /// Gets or sets the time limit in milliseconds.
        /// </summary>
        public int TimeLimitMs { get; set; }
        
        /// <summary>
        /// True if the time limit has passed, false otherwise.
        /// </summary>
        public volatile bool IsOverLimit;
        
        public Stopwatch Stopwatch { get; private set; } = new();
        public ConcurrentBag<Task> RunningTasks { get; private set; } = new();

        public LimitedTimeExecutor(int timeLimitMs)
        {
            TimeLimitMs = timeLimitMs;
        }
        /// <summary>
        /// Runs the specified action and all subtasks created with "StartNewTask" until
        /// the time limit is reached. Continues execution when the time limit is reached.
        /// The tasks will not be stopped but can continue in the background.
        /// </summary>
        /// <param name="action">The first action to complete, which may create subtasks using StartNewTask.</param>
        /// <param name="awaitFirstTask">True if the method should wait for the first task to complete.</param>
        /// <returns></returns>
        public async Task Run(Action action, bool awaitFirstTask = false)
        {
            RunningTasks.Clear();
            IsOverLimit = false;
            Stopwatch.Start();
            var firstTask = StartNewTask(action);
            await Task.Delay(TimeLimitMs);
            if (awaitFirstTask && firstTask != null)
            {
                await firstTask;
            }
            IsOverLimit = true;
            Stopwatch.Stop();
        }
        /// <summary>
        /// Starts a new task if the time limit has not been exceeded yet. Adds the
        /// task to the RunningTasks list.
        /// </summary>
        /// <param name="action">The action to execute in the task.</param>
        /// <returns>The created task, if the time limit has not yet passed.</returns>
        public Task? StartNewTask(Action action)
        {
            Task? task = null;
            if (!IsOverLimit)
            {
                task = Task.Run(action);
                RunningTasks.Add(task);
            }
            return task;
        }
    }
}
