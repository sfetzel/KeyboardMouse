using KeyboardMouseWin.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharpHook;
using System.Windows;

namespace KeyboardMouseWin;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var hostbuilder = new HostBuilder()
        .ConfigureAppConfiguration((context, configurationBuilder) =>
        {
            configurationBuilder.SetBasePath(context.HostingEnvironment.ContentRootPath);
            configurationBuilder.AddJsonFile("appsettings.json", optional: false);
        })
        .ConfigureServices((context, services) =>
        {
            services.Configure<Settings>(context.Configuration);

            services.AddSingleton<TaskPoolGlobalHook>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<CaptionService>();
            services.AddSingleton(sp => 
                new CaptionViewModel(sp.GetRequiredService<CaptionService>(), new FlauiProvider(), sp.GetRequiredService<MainWindow>().Dispatcher));
        })
        .ConfigureLogging(logging =>
        {
            logging.AddConsole();
        });

        var host = hostbuilder.Build();

        host.Start();



        var hook = host.Services.GetRequiredService<TaskPoolGlobalHook>();
        hook.RunAsync();

        var viewModel = host.Services.GetRequiredService< CaptionViewModel>();
        hook.KeyPressed += async (_, e) => await viewModel.HandleKeyDown(SharpHookConverter.ToKey(e.Data.KeyCode), e);
        hook.KeyReleased += (_, e) => viewModel.HandleKeyUp(SharpHookConverter.ToKey(e.Data.KeyCode));

        var window = host.Services.GetRequiredService<MainWindow>();
        window.DataContext = viewModel;


        var application = new Application();
        Task.Run(() => window.Dispatcher.Invoke(() => window.Hide()));
        application.Run(window);
    }
}
