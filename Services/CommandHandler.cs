using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;
    private TranslationCommands _translationCommands;
    public AppConfigs _appConfigs;

    public CommandHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services, AppConfigs appConfigs)
    {
        _client = client;
        _commands = commands;
        _services = services;
        _appConfigs = appConfigs;
    }

    public async Task InitializeAsync()
    {
        // add the public modules that inherit InteractionModuleBase<T> to the InteractionService
        // await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
        var assembly = Assembly.GetExecutingAssembly();
        var modules = assembly.GetTypes().Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(InteractionModuleBase<>));
        foreach (var module in modules)
        {
            await _commands.AddModuleAsync(module, _services);
        }

        // process the InteractionCreated payloads to execute Interactions commands
        _client.InteractionCreated += HandleInteraction;

        // process the command execution results 
        _commands.SlashCommandExecuted += SlashCommandExecuted;
        _commands.ContextCommandExecuted += ContextCommandExecuted;
        _commands.ComponentCommandExecuted += ComponentCommandExecuted;
        _commands.ModalCommandExecuted += ModalCommandExecuted;

        // Add a message received handler
        _client.MessageReceived += HandleMessageReceived;
    }

    private Task ModalCommandExecuted(ModalCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            switch (arg3.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    // implement
                    break;
                case InteractionCommandError.UnknownCommand:
                    // implement
                    break;
                case InteractionCommandError.BadArgs:
                    // implement
                    break;
                case InteractionCommandError.Exception:
                    // implement
                    break;
                case InteractionCommandError.Unsuccessful:
                    // implement
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private Task ComponentCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            switch (arg3.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    // implement
                    break;
                case InteractionCommandError.UnknownCommand:
                    // implement
                    break;
                case InteractionCommandError.BadArgs:
                    // implement
                    break;
                case InteractionCommandError.Exception:
                    // implement
                    break;
                case InteractionCommandError.Unsuccessful:
                    // implement
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private Task ContextCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            switch (arg3.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    // implement
                    break;
                case InteractionCommandError.UnknownCommand:
                    // implement
                    break;
                case InteractionCommandError.BadArgs:
                    // implement
                    break;
                case InteractionCommandError.Exception:
                    // implement
                    break;
                case InteractionCommandError.Unsuccessful:
                    // implement
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            switch (arg3.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    // implement
                    break;
                case InteractionCommandError.UnknownCommand:
                    // implement
                    break;
                case InteractionCommandError.BadArgs:
                    // implement
                    break;
                case InteractionCommandError.Exception:
                    // implement
                    break;
                case InteractionCommandError.Unsuccessful:
                    // implement
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            // create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
            var ctx = new SocketInteractionContext(_client, arg);
            await _commands.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            // if a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (arg.Type == InteractionType.ApplicationCommand)
            {
                await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }

    private async Task HandleMessageReceived(SocketMessage message)
    {
        // Ensure the message is from a user/bot and not a system message
        if (message is not SocketUserMessage userMessage) return;
        var test = _appConfigs.DeeplToken;
        var model = new MessageReplyCommnads(_appConfigs);
        if(userMessage.Author.IsBot) return;
        // Check if the message mentions the bot
        if (userMessage.MentionedUsers.Any(user => user.Id == _client.CurrentUser.Id))
        {
            // 調用 TranslationCommands 的 CheckAndSuggest 方法
            await model.AskAsync(userMessage);
        }
    }
}