
// interation modules must be public and inherit from an IInterationModuleBase
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

public class ExampleCommands : InteractionModuleBase<SocketInteractionContext>
{
    // dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
    public required InteractionService Commands { get; set; }
    private CommandHandler _handler;

    // constructor injection is also a valid way to access the dependecies
    public ExampleCommands(CommandHandler handler)
    {
        _handler = handler;
    }

    // our first /command!
    [SlashCommand("8ball", "輸入問題，獲得答案")]
    public async Task EightBall(string question)
    {
        // create a list of possible replies
        var replies = new List<string>();

        // add our possible replies
        replies.Add("yes");
        replies.Add("no");
        replies.Add("maybe");
        replies.Add("hazzzzy....");

        // get the answer
        var answer = replies[new Random().Next(replies.Count - 1)];

        // reply with the answer
        await RespondAsync($"You asked: [**{question}**], and your answer is: [**{answer}**]");
    }

    [SlashCommand("select", "輸入問題，獲得答案")]
    public async Task JapanIndividualCharacter()
    {
        var builder = new ComponentBuilder()
            .WithSelectMenu(new SelectMenuBuilder()
                .WithPlaceholder("選擇一個選項")
                .WithCustomId("select_menu")
                .AddOption("選項1", "option1")
                .AddOption("選項2", "option2")
                .AddOption("選項3", "option3"));

        await RespondAsync("請選擇一個選項：", components: builder.Build());
    }

    [ComponentInteraction("select_menu")]
    public async Task HandleSelectMenu(string[] selectedOptions)
    {
        // var selectedOption = selectedOptions[0];
        // Console.WriteLine(new LogMessage(LogSeverity.Info, "ExampleCommands : select_menu", $"User: {Context.User.Username}, selected option: {selectedOption}"));
        var modal = new ModalBuilder()
        .WithTitle("測試用 - 回答問題")
        .WithCustomId("modal_input_demo")
        .AddTextInput("Question", "請問...", TextInputStyle.Paragraph)
        .Build();
        await RespondWithModalAsync(modal);
    }

    // Basic slash command. [SlashCommand("name", "description")]
    // Similar to text command creation, and their respective attributes
    [SlashCommand("modal", "Test modal inputs")]
    public async Task ModalInput()
    {
        await RespondWithModalAsync<ExampleModals>("modal_input_demo");
    }

    [ModalInteraction("modal_input_demo")]
    public async Task ModalResponse(ExampleModals modal)
    {
        // 建立要發送的訊息。
        string message = $"{modal.Question}";

        // 指定 AllowedMentions 以避免實際標記所有人。
        AllowedMentions mentions = new();
        // 過濾角色或所有人標記的存在
        mentions.AllowedTypes = AllowedMentionTypes.Users;
        // 建立新的 LogMessage 以使用現有的 Discord.Net LogMessage 參數將所需信息傳遞到控制台
        Console.WriteLine(new LogMessage(LogSeverity.Info, "ExampleModals : modal_input_demo", $"User: {Context.User.Username}, modal input: {message}"));

        // 回應模態。
        await RespondAsync(message, allowedMentions: mentions, ephemeral: true);
    }

}

public class ExampleModals : IModal
{
    public string Title => "測試用 - 回答問題";

    // [InputLabel("說明 : ")]
    // [ModalTextInput("question_input", TextInputStyle.Paragraph, placeholder: "請問...")]
    // public string TextContent { get; set; } = "Send a greeting!";
    // Text box title
    [InputLabel("Send a greeting!")]
    // Strings with the ModalTextInput attribute will automatically become components.
    [ModalTextInput("greeting_input", TextInputStyle.Short, placeholder: "Be nice...", maxLength: 30)]
    // string to hold the user input text
    public string Question { get; set; } = "";
}
