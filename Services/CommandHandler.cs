﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Victoria;

namespace Death_Bot.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly Servers _servers;
        private readonly LavaNode _lavaNode;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration config, Servers servers, LavaNode lavaNode)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            _servers = servers;
            _lavaNode = lavaNode;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _client.ChannelCreated += OnChannelCreated;
            _client.ReactionAdded += OnReactionAdded;
            _client.Ready += OnReadyAsync;

            _service.CommandExecuted += OnCommandExecuted;
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnReadyAsync()
        {
            
            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
            }

            // Other ready related stuff
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.MessageId != 821376730421002270)
            {
                return;
            }

            if (arg3.Emote.Name != "✅")
            {
                return;
            }

            var role = (arg2 as SocketGuildChannel).Guild.Roles.FirstOrDefault(x => x.Id == 821378080873775137);
            await (arg3.User.Value as SocketGuildUser).AddRoleAsync(role);
        }

        private async Task OnChannelCreated(SocketChannel arg)
        {
            if ((arg as ITextChannel) == null) return;
            var channel = arg as ITextChannel;

            await channel.SendMessageAsync("The event was called!");
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            var prefix = await _servers.GetGuildPrefix((message.Channel as SocketGuildChannel).Guild.Id) ?? "$";
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess) await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }
}