using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot
{
    
    class BotMainframe
    {
        //Field Variables
        private const string BUILD_INFO = "v0.0.2"; //Enter Build info here
        private DiscordClient _client;
        //private CommandService commands;
        private string token = "MjMxOTYxMzg3NTQ1Mzk1MjAwDCtIAnA.qFlkW6R88xD5kj8k12oZt5EjLdk"; // Replace with Client ID!!!
        //Error Strings
        private const string notFoundErrStr = "CRITICAL: Required File Directory Not Found! Please Check If You Have The Neccessary Files!";
        private string[] memes;
        private string[] cats;
        private string[] pendingText = new string[500];
        Random rand;
        private const ulong logChannelID = 219193851523497986;
        private ulong[] allowedUserID = {88513309854138368, 170040753425219584, 210507266233860097};
        static void Main(string[] args)
        {
            // Some biolerplate to react to close window event, CTRL-C, kill, etc
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            BotMainframe mainframe = new BotMainframe();
            mainframe.Start();

            //hold the console so it doesn’t run off the end
            while (!exitSystem)
            {
                Thread.Sleep(500);
            }
        }

        static bool exitSystem = false;

        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Please Wait. The Program is Cleaning Up...");

            //do your cleanup here

            //allow main to run off
            exitSystem = true;

            //shutdown right away so there are no lingering threads
            Environment.Exit(0);

            return true;
        }
        #endregion
        public BotMainframe()
        {
           Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);   //Alt Capture Ctrl+C
            try
            {
                memes = Directory.GetFiles(@".\meme");
                cats = Directory.GetFiles(@".\cat");
            }
            catch (DirectoryNotFoundException notFoundEx)
            {
                Console.WriteLine(notFoundErrStr); Console.WriteLine(notFoundErrStr); Console.WriteLine(notFoundErrStr); Console.WriteLine(notFoundErrStr);
                errorReport();
                Console.WriteLine();
                Console.WriteLine("Full Stacktrace Below: ");
                Console.WriteLine(notFoundEx.ToString());
            }
        foreach (var item in memes)
            {
                Console.WriteLine(item.ToString());
            }
            foreach (var item in cats)
            {
                Console.WriteLine(item.ToString());
            }
            rand = new Random();
            Console.WriteLine("Constructor Done");
        }
        public void Start()
        {
            try
            {
                _client = new DiscordClient(x =>
                {
                    x.AppName = "Test";
                    x.AppUrl = "https://fewdpew.me";
                    x.LogLevel = LogSeverity.Verbose;
                    x.LogHandler = Log;
                });

                _client.UsingCommands(x =>
                {
                    x.PrefixChar = '~';
                    x.AllowMentionPrefix = true;
                    x.HelpMode = HelpMode.Public;
                });

                CreateCommands();

                _client.ExecuteAndWait(async () =>
                {
                    await _client.Connect(token, TokenType.Bot);
                    _client.SetGame("Created By Eric1084");
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Oh No. The program have encountered a fatal error... Are you sure you know how to use this program? :/");
                errorReport();
                Console.WriteLine();
                Console.WriteLine("Full Stacktrace Below: ");
                Console.WriteLine(e.ToString());
            }
        }

        public void CreateCommands()
        {
            try
            {
                var cService = _client.GetService<CommandService>();

                cService.CreateCommand("ping")
                    .Description("Returns 'pong'")
                    .Do(async (e) =>
                    {
                        await e.Channel.SendMessage("pong");
                    });
                cService.CreateCommand("hello")
                    .Description("Say hello to a user")
                    .Parameter("user", ParameterType.Unparsed)
                    .Do(async (e) =>
                    {
                        var toReturn = $"Hello {e.GetArg("user")}";
                        await e.Channel.SendMessage(toReturn);
                    });
                cService.CreateCommand("cat")
                    .Description("Sends a cat picture to a channel")
                    .Do(async (e) =>
                    {
                        int randCat = rand.Next(cats.Length);
                        string catToPost = cats[randCat];
                        await e.Channel.SendFile(catToPost);
                        await e.Channel.SendMessage("Here's your cat picture.");
                    });
                cService.CreateCommand("meme")
                    .Description("Really? Ok fine. It sends a random meme.")
                    .Do(async (e) =>
                    {

                        int randMeme = rand.Next(memes.Length);
                        string memeToPost = memes[randMeme];
                        await e.Channel.SendFile(memeToPost);
                        await e.Channel.SendMessage("Here's a random meme :)");
                    });
                cService.CreateCommand("purge")
                    .Description("Purge x number of messages")
                    .Parameter("numToPurge", ParameterType.Required)
                    .Do(async (e) =>
                    {
                        Channel logChannel;
                        int numToPurge = int.Parse(e.GetArg("numToPurge"));
                        if (allowedUserID.Contains(e.User.Id))
                        {
                            await e.Channel.DeleteMessages(await e.Channel.DownloadMessages(numToPurge));
                            await e.Channel.SendMessage("Purging the last " + numToPurge + " messages.");
                            logChannel = e.Server.GetChannel(logChannelID);
                            await logChannel.SendMessage("NOTE: Sensitive Command Have Been Issued!");
                            await logChannel.SendMessage("`" + e.Message.RawText + " ` Have Been Issued By: " + e.Channel.GetUser(e.User.Id));
                        }
                        else
                        {
                            await e.Channel.SendMessage("WARN: " + e.Channel.GetUser(e.User.Id) + " is not included in the trusted user array. ");
                            await e.Channel.SendMessage("`" + e.Message.RawText + " `" + " cannot be executed!");
                        }
                    });
                cService.CreateCommand("say")
                    .Description("Echos String specified")
                    .Parameter("echoMsgString", ParameterType.Required)
                    .Do(async (e) =>
                    {
                        await e.Channel.DeleteMessages(await e.Channel.DownloadMessages(1));
                        await e.Channel.SendMessage(e.GetArg("echoMsgString"));
                    });
                cService.CreateCommand("custom")
                    .Description("Custom Command Remains A Mystery")
                    .Do((e) =>
                    {
                        var user = e.Channel.GetUser(e.User.Id);
                        var userPerms = user.GetPermissions(e.Channel);
                        var role = e.Server.GetRole(232029034572283904);
                        Console.WriteLine(role);
                        user.AddRoles(role);
                    });
                cService.CreateCommand("version")
                    .Description("Displays build and version number")
                    .Do(async (e) =>
                    {
                        await e.Channel.SendMessage(BUILD_INFO);
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine("Unhandled Exception. Something went horribly wrong. Please check with the software developer :D");
                errorReport();
                Console.WriteLine();
                Console.WriteLine("Full Stacktrace Below: ");
                Console.WriteLine(e.ToString());
            }

        }
        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine($"[{e.Severity}][{e.Source}][{e.Message}]");
        }
        private void logCommand()
        { 
        }

        private void errorReport()
        {
            Console.WriteLine();
            Console.WriteLine("Have any problems? Need support? Please contact the developer at eric@hypertech.io");
            Console.WriteLine();
            Console.WriteLine("Press any key to terminate...");
            Console.ReadKey();

        }

        
        protected static void ExitHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("\nThe program has been interrupted.");

            Console.WriteLine("  Key pressed: {0}", args.SpecialKey);
            args.Cancel = true;
            //Console.WriteLine("Please press 'y' to terminate program\n");
            //var cki = Console.ReadKey(true);
            //if (cki.Key == ConsoleKey.Y) 
            Console.WriteLine("Please Wait. The Program is Cleaning Up...");
            Environment.Exit(0);
        }
        
    }
}
