﻿using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using LennyBOTv3.Services;
using LennyBOTv3.Settings;
using Microsoft.Extensions.Options;

namespace LennyBOTv3
{
    public class Bot : LennyBackgroundService<Bot>
    {
        private readonly DiscordSettings _discordSettings;

        public Bot(ILoggerFactory loggerFactory, IOptions<DiscordSettings> discordSettings, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _discordSettings = discordSettings.Value;

            DiscordClient = new DiscordClient(new()
            {
                Token = _discordSettings.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                LoggerFactory = loggerFactory,
            });

            var interactivity = DiscordClient.UseInteractivity(new InteractivityConfiguration()
            {
                AckPaginationButtons = true,
                ButtonBehavior = ButtonPaginationBehavior.Disable,
                PaginationBehaviour = PaginationBehaviour.Ignore,
                Timeout = TimeSpan.FromMinutes(5),
            });

            var commands = DiscordClient.UseCommandsNext(new()
            {
                Services = serviceProvider,
                StringPrefixes = new[] { _discordSettings.Prefix },
            });
            commands.RegisterCommands(Assembly.GetExecutingAssembly());

            // events from https://github.com/Emzi0767/Discord-Companion-Cube-Bot/blob/master/Emzi0767.CompanionCube/CompanionCubeBot.cs
            DiscordClient.Ready += Discord_Ready;
            DiscordClient.GuildDownloadCompleted += Discord_GuildDownloadCompleted;
            DiscordClient.SocketErrored += Discord_SocketErrored;
            DiscordClient.GuildAvailable += Discord_GuildAvailable;
            commands.CommandExecuted += CommandsNext_CommandExecuted;
            commands.CommandErrored += CommandsNext_CommandErrored;
        }

        public DiscordClient DiscordClient { get; }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await DiscordClient.DisconnectAsync();
            DiscordClient.Dispose();
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DiscordClient.ConnectAsync();
            await Task.Delay(-1, stoppingToken);
        }

        private async Task CommandsNext_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            e.Context.Client.Logger.LogError(e.Exception,
                "User '{user}' ({userId}) tried to execute '{command}' in #{channel} ({channelId}) and failed",
                $"{e.Context.User.Username}#{e.Context.User.Discriminator}", e.Context.User.Id, e.Command?.QualifiedName ?? "<unknown command>", e.Context.Channel.Name, e.Context.Channel.Id);

            DiscordEmbedBuilder? embed = null;

            var ex = e.Exception;
            while (ex is AggregateException || ex is TargetInvocationException)
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
                            Description = $"{DiscordEmoji.FromName(e.Context.Client, ":raised_hand:")} You're executing this command too fast, try again in {(int)rcd.TotalMinutes} minutes and {rcd.Seconds} seconds.",
                            Color = new DiscordColor(0xFF0000)
                        };
                    }
                    else
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Title = "Permission denied",
                            Description = $"{DiscordEmoji.FromName(e.Context.Client, ":raised_hand:")} You lack permissions necessary to run this command.",
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

        private Task CommandsNext_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(
                "User '{user}' ({userId}) executed '{command}' in #{channel} ({channelId})",
                $"{e.Context.User.Username}#{e.Context.User.Discriminator}", e.Context.User.Id, e.Command?.QualifiedName ?? "<unknown command>", e.Context.Channel.Name, e.Context.Channel.Id);
            return Task.CompletedTask;
        }

        private Task Discord_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            sender.Logger.LogInformation("Guild available: {guildName}", e.Guild.Name);
            return Task.CompletedTask;
        }

        private Task Discord_GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            Init.SetResult();
            sender.Logger.LogInformation("All guilds are now available");
            return Task.CompletedTask;
        }

        private Task Discord_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation($"Client is ready to process events (responding to '{_discordSettings.Prefix}' prefix)");
            return Task.CompletedTask;
        }

        private Task Discord_SocketErrored(DiscordClient sender, SocketErrorEventArgs e)
        {
            var ex = e.Exception;
            while (ex is AggregateException || ex is TargetInvocationException)
                ex = ex.InnerException;

            sender.Logger.LogCritical(ex, "Socket threw an exception");
            return Task.CompletedTask;
        }
    }
}
