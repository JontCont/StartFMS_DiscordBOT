using DeepL;
using Discord.Commands;
using Discord.WebSocket;
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

    [SlashCommand("trans", "翻譯")]
    [RequireSpecificChannel(1336140909355601920, 1336140668044836955)]
    public async Task Translate(string text, bool isChangeText = true)
    {
        var targetLanguageCode = DetectLanguage(text);

        if (Context.Channel.Id == 1336140668044836955 && targetLanguageCode != "en-US")
        {
            await TranslateText(text, "en-US", "Please provide the translation in the local language", isChangeText);
            return;
        }
        else if (Context.Channel.Id == 1336140909355601920 && targetLanguageCode != LanguageCode.Japanese)
        {
            await TranslateText(text, LanguageCode.Japanese, "Please provide the translation in the local language", isChangeText);
            return;
        }

        await TranslateText(text, LanguageCode.Chinese, "請用正體中文回答", !isChangeText);
    }

    [MessageCommand("Check and Suggest")]
    [RequireSpecificChannel(1336140909355601920, 1336140668044836955)]
    public async Task CheckAndSuggest(SocketUserMessage message)
    {
        if (message.MentionedUsers.Any(user => user.Id == Context.Client.CurrentUser.Id))
        {
            var text = message.Content.Replace(Context.Client.CurrentUser.Mention, "").Trim();
            var targetLanguageCode = DetectLanguage(text);

            var translatedText = await _deepLClient.TranslateTextAsync(
                text,
                sourceLanguageCode: null,
                targetLanguageCode: targetLanguageCode,
                options: new TextTranslateOptions
                {
                    Context = "請提供當地人用語的建議，並列出例子或是解釋"
                }).ConfigureAwait(false);

            var suggestions = translatedText.Text.Split('\n').Select((line, index) => $"{index + 1}. {line}").ToList();
            var response = string.Join("\n", suggestions);

            await RespondAsync($"原文: {text}\n建議:\n{response}", ephemeral: false);
        }
    }

    private async Task TranslateText(string text, string targetLanguageCode, string context, bool isChangeText)
    {
        var translatedText = await _deepLClient.TranslateTextAsync(
            text,
            sourceLanguageCode: null,
            targetLanguageCode: targetLanguageCode,
            options: new TextTranslateOptions
            {
                Context = context
            }).ConfigureAwait(false);

        if (!isChangeText)
            await RespondAsync($"回答前 : {text} , 翻譯後 : {translatedText.Text}", ephemeral: false);
        else
            await RespondAsync(translatedText.Text, ephemeral: false);
    }
    private string DetectLanguage(string input)
    {
        // 檢查是否包含中文字符
        if (input.Any(c => c >= 0x4E00 && c <= 0x9FFF))
        {
            return LanguageCode.Chinese;
        }
        // 檢查是否包含英文字母
        else if (input.Any(c => c >= 0x0041 && c <= 0x007A))
        {
            return "en-US";
        }
        // 檢查是否包含日語字符（平假名、片假名和漢字）
        else if (input.Any(c => (c >= 0x3040 && c <= 0x309F) || // 平假名範圍
                                 (c >= 0x30A0 && c <= 0x30FF) || // 片假名範圍
                                 (c >= 0x4E00 && c <= 0x9FBF))) // 漢字範圍
        {
            return LanguageCode.Japanese;
        }
        return "";
    }

}
