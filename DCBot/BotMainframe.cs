using Discord;
using Discord.Commands;
using Mailgun;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BCBot
{

    class BotMainframe
    {
        //Field Variables
        private const string BUILD_INFO = "v0.0.4-Nightly"; //Enter Build info here
        private Boolean stable = false; //Stable build?
        private DiscordClient _client;
        //private CommandService commands;
        private string token = "MTg4Nzk3NzQyNzQ1NjQ5MTUz.C8IEIg.veLpXZuXQdczTarjuKZgTLtRfIQ"; // Replace with Client ID!!!
        //Error Strings
        private const string notFoundErrStr = "CRITICAL: Required File Directory Not Found! Please Check If You Have The Neccessary Files!";
        private const string STATUSTEXT = " **Server Status**: http://server.mvgd.club:61208/ ";
        //private string configDir = "";
        private string[] memes;
        private string[] cats;
        private string[] pendingText = new string[500];
        Random rand;
        private const ulong logChannelID = 241389217656209409;
        private ulong[] allowedUserID = new ulong[10];
        FileSystemWatcher watcher = new FileSystemWatcher();
        private char prefixCharacter = ' ';
        private Boolean permModOverride = false;
        private string pastCommand = "NULL";
        static void Main(string[] args)
        {
            BotMainframe mainframe = new BotMainframe();
            mainframe.Start();
        }
        public BotMainframe()
        {
            InitializeBot();
        }

        private void InitializeBot()
        {
            Console.WriteLine("Init");
            Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);   //Alt Capture Ctrl+C
            Console.WriteLine("Init");
            var directory = System.AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine("Init");
            directory = directory + @"config";
            Console.WriteLine("Init");
            Console.WriteLine(directory);
            watcher.Path = directory;
            Console.WriteLine("Init");
            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            Console.WriteLine("Init");
            watcher.Filter = "*.txt";
            Console.WriteLine("Init");
            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            Console.WriteLine("Init");
            // Begin watching.
            watcher.EnableRaisingEvents = true;
            Console.WriteLine("InitEnd");
            try
            {
                memes = Directory.GetFiles(@"meme");
                cats = Directory.GetFiles(@"cat");
            }
            catch (DirectoryNotFoundException notFoundEx)
            {
                Console.WriteLine(notFoundErrStr); Console.WriteLine(notFoundErrStr); Console.WriteLine(notFoundErrStr); Console.WriteLine(notFoundErrStr);
                errorReport();
                Console.WriteLine();
                Console.WriteLine("Full Stacktrace Below: ");
                Console.WriteLine(notFoundEx.ToString());
            }
            try
            {
               
            }
            catch(FileNotFoundException fileNotExist)
            {
                Console.WriteLine("Alert: Config File Could Not Be Found. Please Double Check If settings.config exists in the \"config\" folder");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                errorReport();
                Console.WriteLine();
            }
            foreach (var item in memes)
            {
                Console.WriteLine(item.ToString());
            }
            foreach (var item in cats)
            {
                Console.WriteLine(item.ToString());
            }

            int counter = 0; //Stuff for reading
            string line;
            Console.WriteLine("----------------------");
            Console.WriteLine("Admin IDs Detected: ");
            StreamReader file = new StreamReader(@"config/admin.txt");
            while ((line = file.ReadLine()) != null)
            {
                Console.WriteLine(line);
                allowedUserID[counter] = ulong.Parse(line);
                counter++;
            }

            file.Close();

            rand = new Random();
            Console.WriteLine("----------------------");
            Console.WriteLine("Constructor Done");

        }
        public void Start()
        {
            try
            {
                _client = new DiscordClient(x =>
                {
                    x.AppName = "CorporateBot";
                    x.AppUrl = "https://enumc.com";
                    x.LogLevel = LogSeverity.Verbose;
                    x.LogHandler = Log;
                });

                _client.UsingCommands(x =>
                {
                    x.PrefixChar = '#';
                    prefixCharacter = x.PrefixChar.Value;
                    x.AllowMentionPrefix = true;
                    x.HelpMode = HelpMode.Public;
                });

                CreateCommands();

                _client.ExecuteAndWait(async () =>
                {
                    await _client.Connect(token, TokenType.Bot);
                    _client.SetGame("Nightly Build. Unstable!!!");
                    new Task(LoopFeatures).Start();
                    Console.WriteLine("Note: Loop Features Enabled!");
                });
                
            }
            catch (Exception e)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("Oh No. The program have encountered a fatal error... Are you sure you know how to use this program? :/");
                Console.WriteLine("This is a general exception caused during Discord initialization");
                Console.WriteLine("This might be caused by invalid bot token! Press any key to print stack trace.");
                Console.WriteLine("If it says \"401 Unauthorized\", that means you have an invalid token.");
                errorReport();
                Console.WriteLine();
                Console.WriteLine("Full Stacktrace Below: ");
                Console.WriteLine(e.ToString());
                var cki = Console.ReadKey(true);
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
                        int numToPurge = int.Parse(e.GetArg("numToPurge")) + 1;
                        await e.Channel.SendMessage("Please wait..." + " Purging the last " + (numToPurge -1) + " messages.");
                        Thread.Sleep(500);
                        if (allowedUserID.Contains(e.User.Id))
                        {
                            await e.Channel.DeleteMessages(await e.Channel.DownloadMessages(numToPurge+1));
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
                cService.CreateCommand("spam")
                    .Description("Purge x number of messages")
                    .Parameter("spamAmount", ParameterType.Required)
                    .Do(async (e) =>
                    {
                        Channel logChannel;
                        int spamAmount = int.Parse(e.GetArg("spamAmount"));
                        await e.Channel.SendMessage("Please wait...");
                        if (allowedUserID.Contains(e.User.Id))
                        {
                            await e.Channel.SendMessage("Rip Channel. Starting Spam in 3 seconds!!!");
                            for(int count = 0; count < spamAmount; count++)
                            {
                                await e.Channel.SendMessage("Oh yay it works! CONGRADUATIONS!!!");
                            }
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
                cService.CreateCommand("suggest")
                    .Description("Suggest something.")
                    .Parameter("suggestion_goes_here", ParameterType.Required)
                    .Do(async (e) =>
                    {
                        await e.Channel.DownloadMessages(1);
                        //await e.Channel.SendMessage(e.GetArg("suggestion_goes_here"));
                        StreamWriter sw = new StreamWriter(@"config/suggestions.log");
                        sw.WriteLine(e.GetArg("suggestion_goes_here"));
                        sw.Close();
                        await e.Channel.SendMessage(e.User.Mention.ToString() + "Your Suggestion Have Been Logged. Operations have been notified.");
                        Console.WriteLine(e.User.Mention);
                    });
                cService.CreateCommand("approve")
					.Description("Approves a command")
					.Do(async (e) =>
					{
						if (allowedUserID.Contains(e.User.Id))
						{
							await e.Channel.SendMessage("INFO: " + e.Channel.GetUser(e.User.Id) + " have approved " + pastCommand);
							permModOverride = true;
							await e.Channel.SendMessage(prefixCharacter + pastCommand);
						}
						else {
							await e.Channel.SendMessage("INFO: " + e.Channel.GetUser(e.User.Id) + " have insufficient permission.");
						}
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
                cService.CreateCommand("status")
                    .Description("Displays Status Of Server")
                    .Do(async (e) =>
                    {
                        await e.Channel.SendMessage(STATUSTEXT);
                    });
                cService.CreateCommand("version")
                    .Description("Displays build and version number")
                    .Do(async (e) =>
                    {
                        if(!stable)
                        {
                            await e.Channel.SendMessage("Warning: This version is unstable. If you are using this bot frequently, please switch to release branch!");
                        }
                        await e.Channel.SendMessage(BUILD_INFO);
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine("Unhandled Exception. Something went horribly wrong. Please file a bug report with the full stacktrace.");
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
        private void LoopFeatures()
        {
            _client.ExecuteAndWait(async () =>
            {
                //Console.WriteLine("Looped!");
                _client.SetGame("Nightly Build. Unstable!!!");
                Thread.Sleep(120000);
                //Console.WriteLine("Looped!");
                _client.SetGame("Prefix: #");
                Thread.Sleep(120000);
                _client.SetGame("Created By: EnumC");
                Thread.Sleep(120000);
                _client.SetGame(BUILD_INFO);
                Thread.Sleep(60000);
                LoopFeatures();
            });
            
            
        }
        private void errorReport()
        {
            Console.WriteLine();
            Console.WriteLine("Have any problems? Need support? Please contact the developer at eric@enumc.com");
            Console.WriteLine();
            Console.WriteLine("Press any key to terminate/Print Stacktrace");
            Console.ReadKey();
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("----------------------");
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            Console.WriteLine("Config - Admin.txt Have been modified.");
            Console.WriteLine("----------------------");
            InitializeBot();
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
