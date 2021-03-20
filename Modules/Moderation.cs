using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Death_Bot.Modules
{
    public class Moderation : ModuleBase
    {
        [Command("erase")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Erase(int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message = await Context.Channel.SendMessageAsync($"{messages.Count()} messages deleted successfuly!");
            await Task.Delay(100);
            await message.DeleteAsync();
        }
    }
}