using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace LennyBOTv3.Modules
{
    [Group("settings")]
    [Description("")]
    public class SettingsModule : LennyBaseModule
    {
        [Command("location")]
        [Description("")]
        public async Task LocationAsync(CommandContext ctx)
            => await ctx.RespondAsync(await Database.GetUserLocationAsync(ctx.User) ?? "<no location set>");


        [Command("location")]
        [Description("")]
        public async Task LocationAsync(CommandContext ctx,
            [RemainingText, Description("")] string location)
        {
            await Database.SetUserLocationAsync(ctx.User, location);
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client,":white_check_mark:"));
        }
    }
}
