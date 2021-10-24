using DSharpPlus.Entities;

namespace LennyBOTv3.Services
{
    public class RssService : LennyBaseService<RssService>
    {
        public RssService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        internal Task AddFeed(Uri url, DiscordChannel channel, string name)
        {
            throw new NotImplementedException();
        }

        internal Task RemoveFeed(DiscordChannel channel, string name)
        {
            throw new NotImplementedException();
        }

        internal Task<DiscordEmbed> ListFeeds(DiscordChannel channel)
        {
            throw new NotImplementedException();
        }
    }
}
