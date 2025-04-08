using WorkerService;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService() // <--- ключевая строка
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
