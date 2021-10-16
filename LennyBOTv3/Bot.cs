﻿using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using LennyBOTv3.Settings;
using Microsoft.Extensions.Options;

namespace LennyBOTv3
{
    internal class Bot : BackgroundService
    {
        private readonly ILogger<Bot> _logger;
        private readonly DiscordSettings _discordSettings;
        private readonly DiscordClient _discordClient;

        public Bot(ILoggerFactory loggerFactory, IOptions<DiscordSettings> discordSettings, IServiceProvider serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<Bot>();
            _discordSettings = discordSettings.Value;

            _discordClient = new DiscordClient(new()
            {
                Token = _discordSettings.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                LoggerFactory = loggerFactory,
            });
            var interactivity = _discordClient.UseInteractivity();
            var commands = _discordClient.UseCommandsNext(new()
            {
                Services = serviceProvider,
                StringPrefixes = new[] { _discordSettings.Prefix },
            });
            commands.RegisterCommands(Assembly.GetExecutingAssembly());

            // events from https://github.com/Emzi0767/Discord-Companion-Cube-Bot/blob/master/Emzi0767.CompanionCube/CompanionCubeBot.cs
            _discordClient.Ready += Discord_Ready;
            _discordClient.GuildDownloadCompleted += Discord_GuildDownloadCompleted;
            _discordClient.SocketErrored += Discord_SocketErrored;
            _discordClient.GuildAvailable += Discord_GuildAvailable;
            _discordClient.VoiceStateUpdated += Discord_VoiceStateUpdated;
            commands.CommandExecuted += CommandsNext_CommandExecuted;
            commands.CommandErrored += CommandsNext_CommandErrored;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _discordClient.ConnectAsync();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discordClient.DisconnectAsync();
            await base.StopAsync(cancellationToken);
        }

        private Task Discord_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation( "Client is ready to process events");
            return Task.CompletedTask;

            /*if (this.GameTimer == null && !string.IsNullOrWhiteSpace(this.Configuration.Discord.Game))
                this.GameTimer = new Timer(this.GameTimerCallback, sender, TimeSpan.Zero, TimeSpan.FromHours(1));

            using var ssc = this.Services.CreateScope();
            var srv = ssc.ServiceProvider.GetRequiredService<MailmanService>();
            using var db = ssc.ServiceProvider.GetRequiredService<DatabaseContext>();
            await srv.ForceInitializeAsync(db);*/
        }

        private Task Discord_GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            sender.Logger.LogInformation( "All guilds are now available");
            return Task.CompletedTask;

            /*this.Services.GetRequiredService<FeedTimerService>().Start();
            using var ssc = this.Services.CreateScope();
            var srv = ssc.ServiceProvider.GetRequiredService<SchedulerService>();
            await srv.InitializeAsync();*/
        }

        private Task Discord_SocketErrored(DiscordClient sender, SocketErrorEventArgs e)
        {
            var ex = e.Exception;
            while (ex is AggregateException)
                ex = ex.InnerException;

            sender.Logger.LogCritical(ex, "Socket threw an exception");
            return Task.CompletedTask;
        }

        private Task Discord_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            sender.Logger.LogInformation("Guild available: {guildName}", e.Guild.Name);
            return Task.CompletedTask;
        }

        private Task Discord_VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            return Task.CompletedTask;
            /*if (e.User == _discordClient.CurrentUser)
                return;

            var music = this.Services.GetService<MusicService>();
            var gmd = await music.GetOrCreateDataAsync(e.Guild);
            var chn = gmd.Channel;
            if (chn == null || chn != e.Before.Channel)
                return;

            var usrs = chn.Users;
            if (gmd.IsPlaying && !usrs.Any(x => !x.IsBot))
            {
                sender.Logger.LogInformation(LogEvent, $"All users left voice in {e.Guild.Name}, pausing playback", DateTime.Now);
                await gmd.PauseAsync();

                if (gmd.CommandChannel != null)
                    await gmd.CommandChannel.SendMessageAsync($"{DiscordEmoji.FromName(sender, ":play_pause:")} All users left the channel, playback paused. You can resume it by joining the channel and using the `resume` command.");
            }*/
        }

        private Task CommandsNext_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(
                "User '{user}' ({userId}) executed '{command}' in #{channel} ({channelId})",
                $"{e.Context.User.Username}#{e.Context.User.Discriminator}", e.Context.User.Id, e.Command?.QualifiedName ?? "<unknown command>", e.Context.Channel.Name, e.Context.Channel.Id);
            return Task.CompletedTask;
        }

        private async Task CommandsNext_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            e.Context.Client.Logger.LogError(e.Exception,
                "User '{user}' ({userId}) tried to execute '{command}' in #{channel} ({channelId}) and failed", 
                $"{e.Context.User.Username}#{e.Context.User.Discriminator}", e.Context.User.Id, e.Command?.QualifiedName ?? "<unknown command>", e.Context.Channel.Name, e.Context.Channel.Id);

            DiscordEmbedBuilder? embed = null;

            var ex = e.Exception;
            while (ex is AggregateException)
                ex = ex.InnerException;

            if (ex is CommandNotFoundException)
            { } // ignore
            else if (ex is ChecksFailedException cfe)
            {
                if (!cfe.FailedChecks.Any(x => x is RequirePrefixesAttribute))
                {
                    var cooldown = cfe.FailedChecks.OfType<CooldownAttribute>().FirstOrDefault();
                    if (cooldown != null)
                    {
                        var rcd = cooldown.GetRemainingCooldown(e.Context);
                        embed = new DiscordEmbedBuilder
                        {
                            Title = "Ratelimit exceeded",
                            Description = $"{DiscordEmoji.FromName(e.Context.Client, ":raisedhand:")} You're executing this command too fast, try again in {(int)rcd.TotalMinutes} minutes and {rcd.Seconds} seconds.",
                            Color = new DiscordColor(0xFF0000)
                        };
                    }
                    else
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Title = "Permission denied",
                            Description = $"{DiscordEmoji.FromName(e.Context.Client, ":raisedhand:")} You lack permissions necessary to run this command.",
                            Color = new DiscordColor(0xFF0000)
                        };
                    }
                }
            }
            else if (ex is not null)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "A problem occured while executing the command",
                    Description = $"{Formatter.InlineCode(e.Command?.QualifiedName)} threw an exception: {Formatter.InlineCode($"{ex.GetType()}: {ex.Message}")}",
                    Color = new DiscordColor(0xFF0000)
                };
            }

            if (embed != null)
                await e.Context.RespondAsync("", embed: embed.Build());
        }
    }
}
