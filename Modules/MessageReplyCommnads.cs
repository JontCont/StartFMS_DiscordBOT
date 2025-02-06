using System.Text;
using DeepL;
using Discord.WebSocket;

class MessageReplyCommnads : DiscordSocketClient
{
    private DeepLClient _deepLClient;
    public AppConfigs _appConfigs;
    private readonly HttpClient _httpClient;

    public MessageReplyCommnads(AppConfigs appConfigs)
    {
        _appConfigs = appConfigs;
        _deepLClient = new DeepLClient(_appConfigs.DeeplToken);
        _httpClient = new HttpClient();
    }

    ~MessageReplyCommnads()
    {
        _deepLClient.Dispose();
    }


    public async Task AskAsync(SocketUserMessage message)
    {
        var input = message.Content;

        // var translatedText = await _deepLClient.TranslateTextAsync(
        //     input,
        //     sourceLanguageCode: null,
        //     targetLanguageCode: message.Channel.Id == 1336140668044836955 ? "en-US" : "ja-JP",
        //     options: new TextTranslateOptions
        //     {
        //         Context = "請提供當地人用語的建議，並列出例子或是解釋。如果句子有錯請糾正",
        //         PreserveFormatting = true,
        //         Formality = Formality.PreferLess
        //     }).ConfigureAwait(false);

        var correctedText = await CheckGrammarAsync(input).ConfigureAwait(false);

        await message.Channel.SendMessageAsync(correctedText);
    }

    private async Task<string> CheckGrammarAsync(string text)
    {
        HttpContent content = new StringContent($"text={text}&language=auto&enabledOnly=false", Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://api.languagetoolplus.com/v2/check", content);

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonConvert.DeserializeObject<LanguageToolResponse>(responseBody);

        // 處理 LanguageTool 的回應，這裡簡單地返回糾正後的文本
        return ApplyCorrections(text, result!);
    }
    private string ApplyCorrections(string originalText, LanguageToolResponse response)
    {
        // 這裡簡單地應用 LanguageTool 的建議，實際應用中可能需要更複雜的邏輯
        var correctedText = originalText;
        foreach (var match in response.matches)
        {
            foreach (var replacement in match.replacements)
            {
                correctedText = correctedText.Replace(match.context.text.Substring(match.context.offset, match.context.length), replacement.value);
            }
        }
        return correctedText;
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
    public class LanguageToolResponse
    {
        public List<Match> matches { get; set; }
    }

    public class Match
    {
        public Context context { get; set; }
        public List<Replacement> replacements { get; set; }
    }

    public class Context
    {
        public string text { get; set; }
        public int offset { get; set; }
        public int length { get; set; }
    }

    public class Replacement
    {
        public string value { get; set; }
    }
}
