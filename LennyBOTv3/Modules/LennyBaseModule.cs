using DSharpPlus.CommandsNext;

namespace LennyBOTv3.Modules
{
    public class LennyBaseModule : BaseCommandModule
    {
        public override Task BeforeExecutionAsync(CommandContext ctx)
        {
            return ctx.TriggerTypingAsync()
                .ContinueWith(t => base.BeforeExecutionAsync(ctx));
        }
    }
}
