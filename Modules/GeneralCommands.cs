using Discord;
using Discord.Interactions;

class GeneralCommands : InteractionModuleBase<SocketInteractionContext>
{
    public required InteractionService Commands { get; set; }
    private CommandHandler _handler;

    // constructor injection is also a valid way to access the dependecies
    public GeneralCommands(CommandHandler handler)
    {
        _handler = handler;
    }

    [SlashCommand("ping", "回覆 pong!")]
    public async Task Ping()
    {
        await RespondAsync("Pong!", embeds: []);
    }

    [SlashCommand("回應", "隨便回應!!!")]
    public async Task RespondAsync(string content)
    {
        var embed = new EmbedBuilder()
        .WithTitle("嵌入消息標題")
        .WithDescription("這是一個嵌入消息的描述。")
        .WithColor(Color.Blue)
        .WithFooter(footer => footer.Text = "這是底部文字")
        .WithTimestamp(DateTimeOffset.Now)
        .Build();
        await RespondAsync(content, embeds: [embed]);
    }

}