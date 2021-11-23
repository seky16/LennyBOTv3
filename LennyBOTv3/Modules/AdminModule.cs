using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;

namespace LennyBOTv3.Modules
{
    [Group("admin")]
    [Description("Commands for controlling the bot's behaviour.")]
    [RequireOwner, Hidden]
    public sealed class AdministrationModule : LennyBaseModule
    {
        [Command("nick"), Aliases("nickname"), Description("Changes the bot's nickname."), RequireOwner, RequireGuild]
        public async Task NicknameAsync(CommandContext ctx,
            [Description("New nickname for the bot.")] string new_nickname = "")
        {
            if (!ctx.Guild.Members.TryGetValue(ctx.Client.CurrentUser.Id, out var mbr))
                mbr = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            await mbr.ModifyAsync(x =>
            {
                x.Nickname = new_nickname;
                x.AuditLogReason = $"Edited by {ctx.User.Username}#{ctx.User.Discriminator} ({ctx.User.Id})";
            });
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));
        }

        [Command("sql"), Aliases("db")]
        public async Task SqlAsync(CommandContext ctx,
            [RemainingText, Description("")] string query)
            => await ctx.SendPaginatedMessageAsync(await Database.RunQueryAsync(query));

        [Command("sudo"), Description("Executes a command as another user.")]
        public async Task SudoAsync(CommandContext ctx,
            [Description("Member to execute the command as.")] DiscordMember member,
            [RemainingText, Description("Command text to execute.")] string command)
        {
            var cmd = ctx.CommandsNext.FindCommand(command, out var args);
            if (cmd is null)
                throw new CommandNotFoundException(command);

            var fctx = ctx.CommandsNext.CreateFakeContext(member, ctx.Channel, command, ctx.Prefix, cmd, args);
            await ctx.CommandsNext.ExecuteCommandAsync(fctx);
        }
    }
}
