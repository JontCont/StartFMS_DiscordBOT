using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RequireSpecificChannelAttribute : PreconditionAttribute
{
    private readonly List<ulong> _allowedChannelIds;

    public RequireSpecificChannelAttribute(params ulong[] channelIds)
    {
        _allowedChannelIds = new List<ulong>(channelIds);
    }

    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo command, IServiceProvider services)
    {
        if (_allowedChannelIds.Contains(context.Channel.Id))
        {
            return PreconditionResult.FromSuccess();
        }
        else
        {
            await context.Channel.SendMessageAsync("This command can only be used in specific channels.",false);
            return PreconditionResult.FromError("This command can only be used in specific channels.");
        }
    }
}