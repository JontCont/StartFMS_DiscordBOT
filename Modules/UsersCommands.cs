using System.Text;

class UsersCommands : InteractionModuleBase<SocketInteractionContext>
{
    public required InteractionService Commands { get; set; }
    private CommandHandler _handler;
    public AppConfigs _appConfigs;
    readonly string[] _relos = { "職業 : 鑑賞家", "職業 : 盜賊", "職業 : 賭徒", "職業 : 藝術家", "職業 : 設計師", "職業 : 藝人", "職業 : 法師", "職業 : 巫師", "職業 : 魔法師", "職業 : 劍士", "職業 : 咒術師", "職業 : 咒言師", "職業 : 無業戰士" };
    public UsersCommands(CommandHandler handler, AppConfigs appConfigs)
    {
        _handler = handler;
        _appConfigs = appConfigs;
    }

    //抽身分組
    [SlashCommand("抽職業", "抽職業搂")]
    public async Task ReadomRole()
    {
        Random random = new Random();
        int index = random.Next(_relos.Length);
        await RespondAsync($"您抽到 [{_relos[index]}] ", ephemeral: false);

        var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == _relos[index]);
        if (role == null)
        {
            await createUserRole(_relos[index]);
        }
        await JoinUserRole(_relos[index]);
    }

    // 移除所有身分組
    [SlashCommand("移除抽籤所有身分組", "移除所有身分組")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task RemoveAllRole()
    {
        var guild = Context.Guild;
        foreach (var role in guild.Roles)
        {
            if (_relos.Contains(role.Name))
            {
                await role.DeleteAsync();
            }
        }
        await RespondAsync($"已移除所有身分組", ephemeral: true);
    }

    // our first /command!
    [SlashCommand("join", "加入活動票券")]
    public async Task JoinTicket(string ticketId)
    {
        HttpResponseMessage response;
        string content;
        (response, content) = await GetTicketInformation(ticketId);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            await RespondAsync("此票不存在", ephemeral: true);
            return;
        }

        var ticket = JsonConvert.DeserializeObject<TicketArgs>(content);
        if (ticket == null)
        {
            await RespondAsync("此票不存在", ephemeral: true);
        }
        else
        {
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == ticket.role);

            //如果身上有這個身分組就不用再加入
            if (Context.User is IGuildUser user)
            {
                if (role == null)
                {
                    await createUserRole(ticket.role);
                }

                if (role != null && user.RoleIds.Contains(role.Id))
                {
                    await RespondAsync("您已經加入過了", ephemeral: true);
                    return;
                }

                await JoinUserRole(ticket.role);
                await RespondAsync($"您加入了 {ticket.role}", ephemeral: true);
            }
        }

    }

    [SlashCommand("show-tickets", "活動票券")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task TicketList()
    {
        //檢查票券是否已經存在
        HttpResponseMessage response;
        string content;
        (response, content) = await GetTicketInformationList();

        if (response.StatusCode != HttpStatusCode.OK)
        {
            await RespondAsync("票券不存在", ephemeral: true);
        }
        else
        {
            var tickets = JsonConvert.DeserializeObject<List<TicketArgs>>(content);
            if (tickets == null)
            {
                await RespondAsync("票券不存在", ephemeral: true);
            }
            else
            {
                var ticketList = tickets.Select(x => new { x.key, x.role }).ToList();
                var sb = new StringBuilder();
                foreach (var ticket in ticketList)
                {
                    sb.AppendLine(string.Format("票券ID : {0} , 票券名稱 : {1}", ticket.key, ticket.role));
                }

                var embed = new EmbedBuilder()
                    .WithTitle("票券清單")
                    .WithDescription(sb.ToString())
                    .WithColor(Color.Blue)
                    .Build();
                await RespondAsync("以下為票券清單", embed: embed, ephemeral: true);
            }
        }
    }

    [SlashCommand("create-ticket", "新增活動票券")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task AddTicket(string ticketId, string ticketName)
    {
        //檢查票券是否已經存在
        HttpResponseMessage response;
        string content;
        (response, content) = await GetTicketInformation(ticketId);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            await RespondAsync("票券已經存在", ephemeral: true);
            return;
        }

        (response, content) = await CreateTicketPost(ticketId, ticketName);

        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
        {
            await RespondAsync("新增票券成功", ephemeral: true);
        }
        else
        {
            await RespondAsync("新增票券失敗，請確認票券有沒有異常", ephemeral: true);
        }
    }

    [SlashCommand("modify-ticket", "修改活動票券名稱")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task ModifyTicket(string ticketId, string ticketName)
    {
        HttpResponseMessage response;
        string content;

        //檢查票券是否已經存在
        (response, content) = await GetTicketInformation(ticketId);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            await RespondAsync("票券不存在", ephemeral: true);
            return;
        }

        (response, content) = await ModifyTicketName(ticketId, ticketName);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            await RespondAsync("修改票券失敗，請確認票券有沒有異常", ephemeral: true);
        }
        else
        {
            await RespondAsync("修改票券成功", ephemeral: true);
        }
    }

    [SlashCommand("delete-ticket", "刪除活動票券")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task DeleteTicket(string ticketId)
    {
        HttpResponseMessage response;
        string content;

        //檢查票券是否已經存在
        (response, content) = await GetTicketInformation(ticketId);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            await RespondAsync("票券不存在", ephemeral: true);
            return;
        }

        (response, content) = await RemoveTicketFromServer(ticketId);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            await RespondAsync("刪除失敗，請重新嘗試", ephemeral: true);
            return;
        }
        else
        {
            await RespondAsync("刪除票券成功", ephemeral: true);
        }
    }



    // 輸入指令者會加入身分組
    private async Task JoinUserRole(string roleName)
    {
        var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
        var user = Context.User as IGuildUser;
        if (user != null) await user.AddRoleAsync(role);
    }

    //創建身分組
    private async Task<Discord.Rest.RestRole> createUserRole(string roleName)
    {
        var guild = Context.Guild;
        Random random = new Random();
        return await guild.CreateRoleAsync(roleName, color: new Color(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)));
    }

    private async Task<(HttpResponseMessage response, string content)> GetTicketInformationList()
    {
        string apiUrl = $"{_appConfigs.ConnectionUrl}/ticket";
        var client = new HttpClient();
        var response = await client.GetAsync(apiUrl);
        var content = await response.Content.ReadAsStringAsync();
        return (response, content);
    }

    private async Task<(HttpResponseMessage response, string content)> GetTicketInformation(string ticketId)
    {
        string apiUrl = $"{_appConfigs.ConnectionUrl}/ticket/{ticketId}";
        var client = new HttpClient();
        var response = await client.GetAsync(apiUrl);
        var content = await response.Content.ReadAsStringAsync();
        return (response, content);
    }
    private async Task<(HttpResponseMessage response, string content)> CreateTicketPost(string ticketId, string ticketName)
    {
        string apiUrl = $"{_appConfigs.ConnectionUrl}/ticket";
        var client = new HttpClient();
        var response = await client.PostAsync(apiUrl, new StringContent(JsonConvert.SerializeObject(new TicketArgs { key = ticketId, role = ticketName }), Encoding.UTF8, "application/json"));
        var content = await response.Content.ReadAsStringAsync();
        return (response, content);
    }

    private async Task<(HttpResponseMessage response, string content)> ModifyTicketName(string ticketId, string ticketName)
    {
        string apiUrl = $"{_appConfigs.ConnectionUrl}/ticket/{ticketId}";
        var client = new HttpClient();
        var response = await client.PutAsync(apiUrl, new StringContent(JsonConvert.SerializeObject(new TicketArgs { role = ticketName }), Encoding.UTF8, "application/json"));
        var content = await response.Content.ReadAsStringAsync();
        return (response, content);
    }

    private async Task<(HttpResponseMessage response, string content)> RemoveTicketFromServer(string ticketId)
    {
        string apiUrl = $"{_appConfigs.ConnectionUrl}/ticket/{ticketId}";
        var client = new HttpClient();
        var response = await client.DeleteAsync(apiUrl);
        var content = await response.Content.ReadAsStringAsync();
        return (response, content);
    }
}