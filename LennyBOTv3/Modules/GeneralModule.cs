using System.Diagnostics;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace LennyBOTv3.Modules
{
    public class GeneralModule : LennyBaseModule
    {
        public Random Random { private get; set; }

        [Command("clap")]
        [Description("Join text with :clap: emote")]
        public Task ClapCmdAsync(CommandContext ctx,
            [RemainingText, Description("Text to join with :clap: emote")] string text)
        {
            var clap = $" {DiscordEmoji.FromName(ctx.Client, ":clap:")} ";
            return RespondJoined(ctx, text, clap);
        }

        [Command("decide")]
        [Description("Randomly pick one of the options")]
        public async Task DecideCmdAsync(CommandContext ctx,
            [Description("Options to pick from")] params string[] options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            if (options.Length == 0)
                throw new ArgumentException("Options cannot be empty", nameof(options));

            await ctx.RespondAsync(options[Random.Next(options.Length)]);
        }

        [Command("ping")]
        [Description("Get Discord API response time")]
        public async Task PingCmdAsync(CommandContext ctx)
        {
            var ping = ctx.Client.Ping;
            var embed = new DiscordEmbedBuilder()
                .WithTitle($"{DiscordEmoji.FromName(ctx.Client, ":ping_pong:")} Pong!")
                .WithDescription($"Ping: {ping} ms")
                .WithFooter($"Uptime: {GetUptime()}");
            await ctx.RespondAsync(embed);
        }

        [Command("radical")]
        [Description("Join text with :radicalmeme: emote")]
        public Task RadicalCmdAsync(CommandContext ctx,
            [RemainingText, Description("Text to join with :radicalmeme: emote")] string text)
        {
            var clap = $" <:radicalmeme:269806756589207553> "; // this at least doesnt throw
            return RespondJoined(ctx, text, clap);
        }

        private async Task RespondJoined(CommandContext ctx, string text, string separator)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException($"'{nameof(text)}' cannot be null or whitespace.", nameof(text));

            var split = text.ToUpperInvariant().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            text = string.Join(separator, split) + separator;
            var embed = ctx.GetAuthorEmbedBuilder().WithDescription(text);
            await ctx.Channel.SendMessageAsync(embed);
            await ctx.Message.DeleteAsync();
        }
        private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
    }
}
