using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace LennyBOTv3
{
    public static class Extensions
    {
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

        public static string GetNickname(this DiscordUser user) => (user as DiscordMember)?.Nickname ?? user.Username;

        public static DiscordEmbedBuilder GetAuthorEmbedBuilder(this DiscordUser user) => new DiscordEmbedBuilder()
                .WithAuthor(user.GetNickname(), null, user.GetAvatarUrl(DSharpPlus.ImageFormat.Png))
                .WithColor((user as DiscordMember)?.Color ?? DiscordColor.None);
        public static DiscordEmbedBuilder GetAuthorEmbedBuilder(this CommandContext ctx) =>
            GetAuthorEmbedBuilder(ctx.User);

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
    }
}
