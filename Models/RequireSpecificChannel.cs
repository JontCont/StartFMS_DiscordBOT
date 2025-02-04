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

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo command, IServiceProvider services)
    {
        if (_allowedChannelIds.Contains(context.Channel.Id))
        {
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
        else
        {
            return Task.FromResult(PreconditionResult.FromError("This command can only be used in specific channels."));
        }
    }
}