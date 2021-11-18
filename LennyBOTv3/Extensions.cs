using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using SkiaSharp;

namespace LennyBOTv3
{
    public static class Extensions
    {
        // https://social.msdn.microsoft.com/Forums/en-US/fcab7650-18ff-4365-8ef6-f509c0688d0c/drawtext-multiline?forum=xamarinlibraries
        public static void DrawText(this SKCanvas canvas, string text, SKRect area, SKPaint paint)
        {
            var lineHeight = paint.TextSize * 1.2f;
            var lines = SplitLines(text, paint, area.Width);
            var height = lines.Count() * lineHeight;

            var y = area.MidY - (height / 2);

            foreach (var (lineText, width) in lines)
            {
                y += lineHeight;
                var x = area.MidX;
                canvas.DrawText(lineText, x, y, paint);
            }
        }

        public static DiscordEmbedBuilder GetAuthorEmbedBuilder(this DiscordUser user) => new DiscordEmbedBuilder()
                        .WithAuthor(user.GetNickname(), null, user.GetAvatarUrl(DSharpPlus.ImageFormat.Png))
                .WithColor((user as DiscordMember)?.Color ?? DiscordColor.None);

        public static DiscordEmbedBuilder GetAuthorEmbedBuilder(this CommandContext ctx) =>
            GetAuthorEmbedBuilder(ctx.User);

        public static async Task<DiscordMember?> GetDiscordMemberAsync(this DiscordClient client, ulong userId)
        {
            foreach (var (_, guild) in client.Guilds)
            {
                try
                {
                    var member = await guild.GetMemberAsync(userId);
                    if (member is not null)
                        return member;
                }
                catch
                {
                    continue;
                }
            }
            return null;
        }

        public static TService GetHostedService<TService>(this IServiceProvider serviceProvider) where TService : IHostedService
                    => serviceProvider.GetServices<IHostedService>().OfType<TService>().Single();

        public static string GetNickname(this DiscordUser user) => (user as DiscordMember)?.Nickname ?? user.Username;

        public static bool ImplementsInterface(this Type type, Type @interface)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (@interface == null)
                throw new ArgumentNullException(nameof(@interface));

            var interfaces = type.GetInterfaces();
            foreach (var item in interfaces)
            {
                if (@interface.IsGenericTypeDefinition)
                {
                    if (item.IsConstructedGenericType && item.GetGenericTypeDefinition() == @interface)
                    {
                        return true;
                    }
                }
                else
                {
                    if (item == @interface)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static Task MarkSuccessAsync(this CommandContext ctx)
            => ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));

        public static Task SendPaginatedMessageAsync(this CommandContext ctx, IEnumerable<DiscordEmbedBuilder> embeds)
        {
            if (embeds.Count() == 1)
            {
                return ctx.RespondAsync(embed: embeds.First());
            }
            else
            {
                var pages = embeds.Select(x => new Page(null, x));
                return ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages);
            }
        }

        public static string Truncate(this string str, int size, string appendix = "...")
        {
            if (str is null)
                throw new ArgumentNullException(nameof(str));

            if (str.Length <= size)
                return str;
            else
                return string.Concat(str.AsSpan(0, size - appendix.Length), appendix ?? string.Empty);
        }

        private static IEnumerable<(string Text, float Width)> SplitLines(string text, SKPaint paint, float maxWidth)
        {
            var spaceWidth = paint.MeasureText(" ");
            var lines = text.Split('\n');

            return lines.SelectMany((line) =>
            {
                var result = new List<(string Text, float Width)>();

                var words = line.Split(new[] { " " }, StringSplitOptions.None);

                var lineResult = new StringBuilder();
                float width = 0;
                foreach (var word in words)
                {
                    var wordWidth = paint.MeasureText(word);
                    var wordWithSpaceWidth = wordWidth + spaceWidth;
                    var wordWithSpace = word + " ";

                    if (width + wordWidth > maxWidth)
                    {
                        result.Add((lineResult.ToString(), width));
                        lineResult = new StringBuilder(wordWithSpace);
                        width = wordWithSpaceWidth;
                    }
                    else
                    {
                        lineResult.Append(wordWithSpace);
                        width += wordWithSpaceWidth;
                    }
                }

                result.Add((lineResult.ToString(), width));

                return result.ToArray();
            });
        }
    }
}
