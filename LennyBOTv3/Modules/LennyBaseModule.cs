using DSharpPlus.CommandsNext;
using LennyBOTv3.Services;

namespace LennyBOTv3.Modules
{
    public abstract class LennyBaseModule : BaseCommandModule
    {
        protected DatabaseService Database => ServiceProvider.GetHostedService<DatabaseService>();
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public IServiceProvider ServiceProvider { protected get; set; } // property injection
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public override Task BeforeExecutionAsync(CommandContext ctx)
        {
            return ctx.TriggerTypingAsync()
                .ContinueWith(t => base.BeforeExecutionAsync(ctx));
        }
    }
}
