using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using Discord.Rest;
using System.Diagnostics;

class Wokers
{
    // setup our fields we assign later
    private static AppConfigs _appConfigs = null!;
    private DiscordSocketClient _client = null!;
    private DiscordRestClient _restClient = null!;
    private InteractionService _commands = null!;
    private ulong _testGuildId;

    public Wokers()
    {
        // 確認有沒有 appsettings.json
        Console.WriteLine("appsettings Exists : " + File.Exists("appsettings.json"));
        
        var _builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(path: "appsettings.json");

        // build the configuration and assign to _config          
        var config = _builder.Build();
        _appConfigs = config.Get<AppConfigs>() ?? new AppConfigs();
        _testGuildId = ulong.Parse(_appConfigs.TestGuildId);
        _restClient = new DiscordRestClient();
    }

    public async Task RunAsync()
    {
        // call ConfigureServices to create the ServiceCollection/Provider for passing around the services
        using (var services = ConfigureServices())
        {
            // get the rest client and login
            await _restClient.LoginAsync(TokenType.Bot, _appConfigs.Token);

            // get the client and assign to client 
            // you get the services via GetRequiredService<T>
            var client = services.GetRequiredService<DiscordSocketClient>();
            var commands = services.GetRequiredService<InteractionService>();
            _client = client;
            _commands = commands;

            // setup logging and the ready event
            client.Log += LogAsync;
            commands.Log += LogAsync;
            client.Ready += ReadyAsync;

            // this is where we get the Token value from the configuration file, and start the bot
            await client.LoginAsync(TokenType.Bot, _appConfigs.Token);
            await client.StartAsync();

            // we get the CommandHandler class here and call the InitializeAsync method to start things up for the CommandHandler service
            await services.GetRequiredService<CommandHandler>().InitializeAsync();

            await Task.Delay(Timeout.Infinite);
        }
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        // this method will add commands globally, but can take around an hour
        await _restClient.DeleteAllGlobalCommandsAsync();
        if (IsInDevelopment())
        {
            // this is where you put the id of the test discord guild
            System.Console.WriteLine($"In debug mode, adding commands to {_testGuildId}...");
            await _commands.RegisterCommandsToGuildAsync(_testGuildId);
        }
        else
        {
            await _commands.RegisterCommandsGloballyAsync(true);
        }
        Console.WriteLine($"Connected as -> [{_client.CurrentUser}] :)");
    }

    // this method handles the ServiceCollection creation/configuration, and builds out the service provider we can call on later
    private ServiceProvider ConfigureServices()
    {
        // 這裡返回一個 ServiceProvider，稍後用於調用這些服務
        // 我們可以在這裡添加我們可以訪問的類型，因此添加新的 using 語句：
        return new ServiceCollection()
            .AddSingleton(_appConfigs)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<CommandHandler>()
            .BuildServiceProvider();
    }

    static bool IsInDevelopment()
    {
        var env = _appConfigs.Environment;
        Console.WriteLine($"Environment: {env}");
        if (env == null)
        {
            return false;
        }
        if (env.ToLower() == "development")
        {
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        _client.Dispose();
        _restClient.Dispose();
        _commands.Dispose();
    }

    ~Wokers()
    {
        Dispose();
    }
}