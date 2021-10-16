using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace LennyBOTv3.Modules
{
    public class GeneralModule : BaseCommandModule
    {

        [Command("ping"), Description("Check API connection status.")]
        public async Task PingAsync(CommandContext ctx)
        {
            throw new NotImplementedException();
            await ctx.RespondAsync($"pong {ctx.Client.Ping} ms");
        }

        public override async Task BeforeExecutionAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await base.BeforeExecutionAsync(ctx);
        }
    }
}
