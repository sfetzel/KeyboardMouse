using KeyboardMouseWin.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardMouseWin.Test
{
    [TestClass]
    public class LimitedTimeExecutorTest
    {
        private int timeLimit = 50;

        [TestInitialize]
        public void Initialize()
        {
            // Start executor once for JIT.
            var executor = new LimitedTimeExecutor(10);
            executor.StartNewTask(() => { });
        }

        [TestMethod]
        public async Task TestRun_WhenTaskTakesLongerThanLimit_ShouldNotExceedLimit()
        {
            var executor = new LimitedTimeExecutor(timeLimit);
            var stopwatch = new Stopwatch();
            await executor.Run(async () => await Task.Delay(timeLimit + 30));
            Assert.IsTrue(executor.RunningTasks.First().IsCompleted);
        }

        [TestMethod]
        public async Task TestRun_WhenAwaitFirstTask_ShouldExceedLimit()
        {
            var executor = new LimitedTimeExecutor(timeLimit);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await executor.Run(() => Thread.Sleep(timeLimit * 100));
            stopwatch.Stop();
            AssertTime(stopwatch);
            var firstTask = executor.RunningTasks.First();
            Assert.IsFalse(firstTask.IsCompleted);
        }

        [TestMethod]
        public async Task TestRun_WhenSubtaskTakesLongerThanLimit_ShouldNotExceedLimit()
        {
            var executor = new LimitedTimeExecutor(timeLimit);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await executor.Run(() =>
            {
                Task.Delay(timeLimit / 10 * 8);
                executor.StartNewTask(async () => await Task.Delay(timeLimit * 2));
            });
            stopwatch.Stop();
            AssertTime(stopwatch);
        }

        [TestMethod]
        public async Task TestRun_WhenSubtaskTakesLongerThanLimit_IsOverLimitShouldBeTrue()
        {
            var executor = new LimitedTimeExecutor(timeLimit);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await executor.Run(async () =>
            {
                await Task.Delay(timeLimit / 10 * 8);
                executor.StartNewTask(async () =>
                {
                    await Task.Delay(timeLimit * 10);
                    if (!executor.IsOverLimit)
                    {
                        throw new AssertFailedException();
                    }
                });
            });
            stopwatch.Stop();
            AssertTime(stopwatch);
            Assert.IsTrue(executor.IsOverLimit);
            Task.WaitAll(executor.RunningTasks.ToArray());
        }

        private void AssertTime(Stopwatch stopwatch)
        {
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < timeLimit * 10 / 5, $"Actual time: {stopwatch.ElapsedMilliseconds}");
        }
    }
}
