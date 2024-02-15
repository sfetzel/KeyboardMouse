// See https://aka.ms/new-console-template for more information
using System.Collections.Concurrent;
using System.Diagnostics;

Console.WriteLine("Hello, World!");

async Task MySuperSlowTask(ConcurrentBag<string> results, CancellationToken token)
{
    await Task.Delay(results.Count * 20);
    results.Add("supa" + results.Count);
    if (!token.IsCancellationRequested)
    {
        await MySuperSlowTask(results, token);
        await MySuperSlowTask(results, token);
    }
}

var cancellationToken = new CancellationTokenSource();
var token = cancellationToken.Token;
var results = new ConcurrentBag<string>();
var stopwatch = new Stopwatch();
stopwatch.Start();
var task = Task.Run(async () =>
{
    await MySuperSlowTask(results, token);
}, token);
cancellationToken.CancelAfter(50);
Thread.Sleep(50);
stopwatch.Stop();
Console.WriteLine($"Task finished in {stopwatch.ElapsedMilliseconds}ms");
foreach (var result in results)
{
    Console.WriteLine(result);
}