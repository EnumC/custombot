using Discord;
using Discord.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    class MyBot
    {
        DiscordClient discord;

        public MyBot()
        {

            discord = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            discord.ExecuteAndWait(async () =>
            {
                await discord.Connect("MjMxOTYxMzg3NTQ1Mzk1MjAw.CtIAnA.qFlkW6R88xD5kj8k12oZt5EjLdk", TokenType.Bot);
            });
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
