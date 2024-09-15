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

    [SlashCommand("回應", "隨便回應!!!")]
    public async Task RespondAsync()
    {
        var embed = new EmbedBuilder()
        .WithTitle("嵌入消息標題")
        .WithDescription("這是一個嵌入消息的描述。")
        .WithColor(Color.Blue)
        .WithFooter(footer => footer.Text = "這是底部文字")
        .WithTimestamp(DateTimeOffset.Now)
        .Build();
        await RespondAsync("這是範本", embeds: [embed], ephemeral: true);
    }

    // -- 語音 -- //

    // 產生語音頻道
    [SlashCommand("創建語音", "建立語音頻道")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task CreateVoiceChannel(string 頻道名稱, string? 指定分類 = null, string? 擁有身分組 = null)
    {
        var guild = Context.Guild;

        if (指定分類 != null)
        {
            var category = guild.CategoryChannels.FirstOrDefault(x => x.Name == 指定分類);
            if (category == null)
            {
                await RespondAsync($"找不到名為 {指定分類} 的分類。", embeds: [], ephemeral: true);
                return;
            }
            var voiceChannel = await guild.CreateVoiceChannelAsync(頻道名稱, x => x.CategoryId = category.Id);
            await RespondAsync($"已建立語音頻道：{voiceChannel.Name}", embeds: [], ephemeral: true);
        }
        else
        {
            var voiceChannel = await guild.CreateVoiceChannelAsync(頻道名稱);
            await RespondAsync($"已建立語音頻道：{voiceChannel.Name}", embeds: [], ephemeral: true);
        }

        if (擁有身分組 != null)
        {
            await this.AddVoiceChannelToRoleMaxPermission(頻道名稱, 擁有身分組);
        }
    }

    // 語音頻道加入擁有身分組最大權限
    [SlashCommand("語音最大權限", "語音頻道加入擁有身分組最大權限")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task AddVoiceChannelToRoleMaxPermission(string 頻道名稱, string 角色名稱)
    {
        var guild = Context.Guild;
        var voiceChannel = guild.VoiceChannels.FirstOrDefault(x => x.Name == 頻道名稱);
        if (voiceChannel == null)
        {
            await RespondAsync($"找不到名為 {頻道名稱} 的語音頻道。", embeds: [], ephemeral: true);
            return;
        }
        var role = guild.Roles.FirstOrDefault(x => x.Name == 角色名稱);
        if (role == null)
        {
            await RespondAsync($"找不到名為 {角色名稱} 的角色。", embeds: [], ephemeral: true);
            return;
        }
        await voiceChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(connect: PermValue.Allow, speak: PermValue.Allow, stream: PermValue.Allow, muteMembers: PermValue.Allow, deafenMembers: PermValue.Allow, moveMembers: PermValue.Allow, useVoiceActivation: PermValue.Allow, prioritySpeaker: PermValue.Allow));
        await RespondAsync($"已將語音頻道 {voiceChannel.Name} 加入角色 {role.Name} 最大權限", embeds: [], ephemeral: true);
    }

    // 語音頻道加入身分組
    [SlashCommand("語音加入身分組", "語音頻道加入身分組")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task AddVoiceChannelToRole(string 頻道名稱, string 角色名稱)
    {
        var guild = Context.Guild;
        var voiceChannel = guild.VoiceChannels.FirstOrDefault(x => x.Name == 頻道名稱);
        if (voiceChannel == null)
        {
            await RespondAsync($"找不到名為 {頻道名稱} 的語音頻道。", embeds: [], ephemeral: true);
            return;
        }
        var role = guild.Roles.FirstOrDefault(x => x.Name == 角色名稱);
        if (role == null)
        {
            await RespondAsync($"找不到名為 {角色名稱} 的角色。", embeds: [], ephemeral: true);
            return;
        }
        await voiceChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(connect: PermValue.Allow));
        await RespondAsync($"已將語音頻道 {voiceChannel.Name} 加入角色 {role.Name}", embeds: [], ephemeral: true);
    }

    // 修改語音頻道名稱
    [SlashCommand("修改語音", "修改語音頻道名稱")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task ModifyVoiceChannel(string 頻道名稱, string 新名稱)
    {
        var guild = Context.Guild;
        var voiceChannel = guild.VoiceChannels.FirstOrDefault(x => x.Name == 頻道名稱);
        if (voiceChannel == null)
        {
            await RespondAsync($"找不到名為 {頻道名稱} 的語音頻道。", embeds: [], ephemeral: true);
            return;
        }
        await voiceChannel.ModifyAsync(x => x.Name = 新名稱);
        await RespondAsync($"已修改語音頻道名稱：{voiceChannel.Name} -> {新名稱}", embeds: [], ephemeral: true);
    }

    // 刪除語音頻道
    [SlashCommand("刪除語音", "刪除語音頻道")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task DeleteVoiceChannel(string 頻道名稱)
    {
        var guild = Context.Guild;
        var voiceChannel = guild.VoiceChannels.FirstOrDefault(x => x.Name == 頻道名稱);
        if (voiceChannel == null)
        {
            await RespondAsync($"找不到名為 {頻道名稱} 的語音頻道。", embeds: [], ephemeral: true);
            return;
        }
        await voiceChannel.DeleteAsync();
        await RespondAsync($"已刪除語音頻道：{voiceChannel.Name}", embeds: [], ephemeral: true);
    }


    //--  文字 -- //
    // 產生文字頻道
    [SlashCommand("create-text", "建立文字頻道")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task CreateTextChannel(string 頻道名稱, string? 指定分類 = null, string? 擁有身分組 = null)
    {
        var guild = Context.Guild;
        if (指定分類 != null)
        {
            var category = guild.CategoryChannels.FirstOrDefault(x => x.Name == 指定分類);
            if (category == null)
            {
                await RespondAsync($"找不到名為 {指定分類} 的分類。", embeds: [], ephemeral: true);
                return;
            }
            var textChannel = await guild.CreateTextChannelAsync(頻道名稱, x => x.CategoryId = category.Id);
            await RespondAsync($"已建立文字頻道：{textChannel.Name}", embeds: [], ephemeral: true);
        }
        else
        {
            var textChannel = await guild.CreateTextChannelAsync(頻道名稱);
            await RespondAsync($"已建立文字頻道：{textChannel.Name}", embeds: [], ephemeral: true);
        }
    }

    // 文字頻道加入擁有身分組最大權限
    [SlashCommand("文字最大權限", "文字頻道加入擁有身分組最大權限")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task AddTextChannelToRoleMaxPermission(string 頻道名稱, string 角色名稱)
    {
        var guild = Context.Guild;
        var textChannel = guild.TextChannels.FirstOrDefault(x => x.Name == 頻道名稱);
        if (textChannel == null)
        {
            await RespondAsync($"找不到名為 {頻道名稱} 的文字頻道。", embeds: [], ephemeral: true);
            return;
        }
        var role = guild.Roles.FirstOrDefault(x => x.Name == 角色名稱);
        if (role == null)
        {
            await RespondAsync($"找不到名為 {角色名稱} 的角色。", embeds: [], ephemeral: true);
            return;
        }
        await textChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, manageRoles: PermValue.Allow));
        await RespondAsync($"已將文字頻道 {textChannel.Name} 加入角色 {role.Name} 最大權限", embeds: [], ephemeral: true);
    }

    // 文字頻道加入身分組
    [SlashCommand("文字加入身分組", "文字頻道加入身分組")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task AddTextChannelToRole(string 頻道名稱, string 角色名稱)
    {
        var guild = Context.Guild;
        var textChannel = guild.TextChannels.FirstOrDefault(x => x.Name == 頻道名稱);
        if (textChannel == null)
        {
            await RespondAsync($"找不到名為 {頻道名稱} 的文字頻道。", embeds: [], ephemeral: true);
            return;
        }
        var role = guild.Roles.FirstOrDefault(x => x.Name == 角色名稱);
        if (role == null)
        {
            await RespondAsync($"找不到名為 {角色名稱} 的角色。", embeds: [], ephemeral: true);
            return;
        }
        await textChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Allow));
        await RespondAsync($"已將文字頻道 {textChannel.Name} 加入角色 {role.Name}", embeds: [], ephemeral: true);
    }

    // 修改文字頻道名稱
    [SlashCommand("修改文字頻道名稱", "修改文字頻道名稱")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task ModifyTextChannel(string 頻道名稱, string 新名稱)
    {
        var guild = Context.Guild;
        var textChannel = guild.TextChannels.FirstOrDefault(x => x.Name == 頻道名稱);
        if (textChannel == null)
        {
            await RespondAsync($"找不到名為 {頻道名稱} 的文字頻道。", embeds: [], ephemeral: true);
            return;
        }
        await textChannel.ModifyAsync(x => x.Name = 新名稱);
        await RespondAsync($"已修改文字頻道名稱：{textChannel.Name} -> {新名稱}", embeds: [], ephemeral: true);
    }

    // 刪除文字頻道
    [SlashCommand("刪除文字頻道", "刪除文字頻道")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task DeleteTextChannel(string 頻道名稱)
    {
        var guild = Context.Guild;
        var textChannel = guild.TextChannels.FirstOrDefault(x => x.Name == 頻道名稱);
        if (textChannel == null)
        {
            await RespondAsync($"找不到名為 {頻道名稱} 的文字頻道。", embeds: [], ephemeral: true);
            return;
        }
        await textChannel.DeleteAsync();
        await RespondAsync($"已刪除文字頻道：{textChannel.Name}", embeds: [], ephemeral: true);
    }


    //--  分類 -- //
    // 產生分類
    [SlashCommand("create-category", "建立分類")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task CreateCategory(string categoryName)
    {
        var guild = Context.Guild;
        var category = await guild.CreateCategoryChannelAsync(categoryName);
        await RespondAsync($"已建立分類：{category.Name}");
    }

    // 修改分類名稱
    [SlashCommand("modify-category", "修改分類名稱")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task ModifyCategory(string categoryName, string newCategoryName)
    {
        var guild = Context.Guild;
        var category = guild.CategoryChannels.FirstOrDefault(x => x.Name == categoryName);
        if (category == null)
        {
            await RespondAsync($"找不到名為 {categoryName} 的分類。", embeds: [], ephemeral: true);
            return;
        }
        await category.ModifyAsync(x => x.Name = newCategoryName);
        await RespondAsync($"已修改分類名稱：{category.Name} -> {newCategoryName}", embeds: [], ephemeral: true);
    }

    // 刪除分類
    [SlashCommand("delete-category", "刪除分類")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task DeleteCategory(string categoryName)
    {
        var guild = Context.Guild;
        var category = guild.CategoryChannels.FirstOrDefault(x => x.Name == categoryName);
        if (category == null)
        {
            await RespondAsync($"找不到名為 {categoryName} 的分類。", embeds: [], ephemeral: true);
            return;
        }
        await category.DeleteAsync();
        await RespondAsync($"已刪除分類：{category.Name}", embeds: [], ephemeral: true);
    }

    // -- 角色 -- //

    // 產生角色
    [SlashCommand("create-role", "建立角色")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task CreateRole(string roleName)
    {
        var guild = Context.Guild;
        var role = await guild.CreateRoleAsync(roleName);
        await RespondAsync($"已建立角色：{role.Name}");
    }

    // 修改角色名稱
    [SlashCommand("modify-role", "修改角色名稱")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task ModifyRole(string roleName, string newRoleName)
    {
        var guild = Context.Guild;
        var role = guild.Roles.FirstOrDefault(x => x.Name == roleName);
        if (role == null)
        {
            await RespondAsync($"找不到名為 {roleName} 的角色。", embeds: [], ephemeral: true);
            return;
        }
        await role.ModifyAsync(x => x.Name = newRoleName);
        await RespondAsync($"已修改角色名稱：{role.Name} -> {newRoleName}", embeds: [], ephemeral: true);
    }

    // 刪除角色
    [SlashCommand("delete-role", "刪除角色")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task DeleteRole(string roleName)
    {
        var guild = Context.Guild;
        var role = guild.Roles.FirstOrDefault(x => x.Name == roleName);
        if (role == null)
        {
            await RespondAsync($"找不到名為 {roleName} 的角色。", embeds: [], ephemeral: true);
            return;
        }
        await role.DeleteAsync();
        await RespondAsync($"已刪除角色：{role.Name}", embeds: [], ephemeral: true);
    }


    //抽身分組
    [SlashCommand("抽職業", "抽職業搂")]
    public async Task ReadomRole()
    {
        string[] relos = { "職業 : 鑑賞家", "職業 : 盜賊", "職業 : 賭徒", "職業 : 藝術家", "職業 : 設計師", "職業 : 藝人", "職業 : 法師", "職業 : 巫師", "職業 : 魔法師", "職業 : 劍士", "職業 : 咒術師", "職業 : 咒言師", "職業 : 無業戰士" };
        Random random = new Random();
        int index = random.Next(relos.Length);
        await RespondAsync($"您抽到 [{relos[index]}] ", embeds: [], ephemeral: false);

        var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == relos[index]);
        if (role == null)
        {
            await createUserRole(relos[index]);
        }
        await JoinUserRole(relos[index]);
    }



    // 輸入指令者會加入身分組
    public async Task JoinUserRole(string roleName)
    {
        var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
        var user = Context.User as IGuildUser;
        if (user != null) await user.AddRoleAsync(role);
    }

    //創建身分組
    public async Task createUserRole(string roleName)
    {
        var guild = Context.Guild;
        Random random = new Random();
        var role = await guild.CreateRoleAsync(roleName, color: new Color(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)));
    }

    // 移除所有身分組
    [SlashCommand("移除抽籤所有身分組", "移除所有身分組")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task RemoveAllRole()
    {
        string[] relos = { "職業 : 鑑賞家", "職業 : 盜賊", "職業 : 賭徒", "職業 : 藝術家", "職業 : 設計師", "職業 : 藝人", "職業 : 法師", "職業 : 巫師", "職業 : 魔法師", "職業 : 劍士", "職業 : 咒術師", "職業 : 咒言師", "職業 : 無業戰士" };
        var guild = Context.Guild;
        foreach (var role in guild.Roles)
        {
            if (relos.Contains(role.Name))
            {
                await role.DeleteAsync();
            }
        }
        await RespondAsync($"已移除所有身分組", embeds: [], ephemeral: true);
    }
}