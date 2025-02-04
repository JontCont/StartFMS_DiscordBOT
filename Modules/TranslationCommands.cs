using DeepL;
using Discord.Commands;
using System.Threading.Tasks;

class TranslationCommands : InteractionModuleBase<SocketInteractionContext>
{

    public required InteractionService Commands { get; set; }
    private CommandHandler _handler;
    private DeepLClient _deepLClient;
    public AppConfigs _appConfigs;

    public TranslationCommands(CommandHandler handler, AppConfigs appConfigs)
    {
        _handler = handler;
        _appConfigs = appConfigs;
        _deepLClient = new DeepLClient(_appConfigs.DeeplToken);
    }

    ~TranslationCommands()
    {
        _deepLClient.Dispose();
    }

    [SlashCommand("translate", "翻譯")]
    [RequireSpecificChannel(1336140668044836955, 1336140909355601920)]
    public async Task AskAsync(string text, bool isChangeText = true)
    {
        var translatedText = new {
            Text = text,
        }; 
        // var translatedText = await _deepLClient.TranslateTextAsync(
        //     text,
        //     sourceLanguageCode: null,
        //     targetLanguageCode: "en-US")
        //     .ConfigureAwait(false);
        if (!isChangeText)
            await RespondAsync($"回答前 : {text} , 翻譯後 : {translatedText.Text}", ephemeral: false);
        else
            await RespondAsync(translatedText.Text, ephemeral: false);
    }

}
