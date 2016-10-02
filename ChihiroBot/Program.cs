﻿using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using ChihiroBot.Modules.Admin;
using ChihiroBot.Modules.Border;
using ChihiroBot.Modules.Colors;
using ChihiroBot.Modules.Feeds;
using ChihiroBot.Modules.Finance;
using ChihiroBot.Modules.Gif;
using ChihiroBot.Modules.Github;
using ChihiroBot.Modules.Modules;
using ChihiroBot.Modules.N_des;
using ChihiroBot.Modules.Public;
using ChihiroBot.Modules.Random;
using ChihiroBot.Modules.StarlightStage;
using ChihiroBot.Modules.Status;
using ChihiroBot.Modules.Timer;
using ChihiroBot.Modules.Twitch;
using ChihiroBot.Services;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ChihiroBot
{
    public class Program
    {
        public static void Main(string[] args) => new Program().Start(args);

        private const string AppName = "ChihiroBot";
        private const string AppUrl = "https://github.com/RiskCC/ChihiroBot";
        private string LogFile = "./config/ChihiroBot.log";

        private DiscordClient _client;

        private void Start(string[] args)
        {
#if !DNXCORE50
            Console.Title = $"{AppName} (Discord.Net v{DiscordConfig.LibVersion})";
#endif

            GlobalSettings.Load();

            _client = new DiscordClient(x =>
            {
                x.AppName = AppName;
                x.AppUrl = AppUrl;
                x.MessageCacheSize = 0;
                x.UsePermissionsCache = true;
                x.EnablePreUpdateEvents = true;
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = OnLogMessage;
            })
            .UsingCommands(x =>
            {
                x.AllowMentionPrefix = true;
                x.HelpMode = HelpMode.Public;
                x.ExecuteHandler = OnCommandExecuted;
                x.ErrorHandler = OnCommandError;
            })
            .UsingModules()
            .UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;
                //x.EnableMultiserver = true;
                x.EnableEncryption = true;
                x.Bitrate = AudioServiceConfig.MaxBitrate;
                x.BufferLength = 10000;
            })
            .UsingPermissionLevels(PermissionResolver);

            _client.AddService<SettingsService>();
            _client.AddService<HttpService>();

            // Modules module
            _client.AddModule<ModulesModule>("Modules", ModuleFilter.None);

            // Core custom modules
            _client.AddModule<BorderModule>("Border", ModuleFilter.None);
            _client.AddModule<RandomModule>("Random", ModuleFilter.None);
            _client.AddModule<TimerModule>("Timer", ModuleFilter.None);
            _client.AddModule<StarlightStageModule>("StarlightStage", ModuleFilter.None);

            // DiscordBot built-in modules
            _client.AddModule<AdminModule>("Admin", ModuleFilter.ServerWhitelist);
            _client.AddModule<ColorsModule>("Colors", ModuleFilter.ServerWhitelist);
            _client.AddModule<FeedModule>("Feeds", ModuleFilter.ServerWhitelist);
            _client.AddModule<GithubModule>("Repos", ModuleFilter.ServerWhitelist);
            _client.AddModule<PublicModule>("Public", ModuleFilter.None);
            _client.AddModule<TwitchModule>("Twitch", ModuleFilter.ServerWhitelist);
            _client.AddModule<StatusModule>("Status", ModuleFilter.ServerWhitelist);

            // Misc custom modules
            _client.AddModule<FinanceModule>("Finance", ModuleFilter.ServerWhitelist);
            _client.AddModule<GifModule>("Gif", ModuleFilter.None);
            _client.AddModule<N_desModule>("N_des", ModuleFilter.None);

            //_client.AddModule(new ExecuteModule(env, exporter), "Execute", ModuleFilter.ServerWhitelist);

#if PRIVATE
            PrivateModules.Install(_client);
#endif

            //Convert this method to an async function and connect to the server
            //DiscordClient will automatically reconnect once we've established a connection, until then we loop on our end
            //Note: ExecuteAndWait is only needed for Console projects as Main can't be declared as async. UI/Web applications should *not* use this function.
            _client.ExecuteAndWait(async () =>
            {
                while (true)
                {
                    try
                    {
                        await _client.Connect(GlobalSettings.Discord.Email, GlobalSettings.Discord.Password);
                        //_client.SetGame("debugging, sorry!");
                        //_client.SetGame("good to go~");
                        //await _client.ClientAPI.Send(new Discord.API.Client.Rest.HealthRequest());
                        break;
                    }
                    catch (Exception ex)
                    {
                        _client.Log.Error($"Login Failed", ex);
                        await Task.Delay(_client.Config.FailedReconnectDelay);
                    }
                }
            });
        }

        private void OnCommandError(object sender, CommandErrorEventArgs e)
        {
            string msg = e.Exception?.Message;
            if (msg == null) //No exception - show a generic message
            {
                switch (e.ErrorType)
                {
                    case CommandErrorType.Exception:
                        msg = "Unknown error. ಠ╭╮ಠ";
                        break;
                    case CommandErrorType.BadPermissions:
                        msg = "You do not have permission to run this command. (;﹏;)";
                        break;
                    case CommandErrorType.BadArgCount:
                        msg = "You provided the incorrect number of arguments for this command. （；￣д￣）";
                        break;
                    case CommandErrorType.InvalidInput:
                        msg = "Unable to parse your command, please check your input. o(TヘTo)";
                        break;
                    case CommandErrorType.UnknownCommand:
                        msg = "Unknown command. .·´¯`(>▂<)´¯`·.";
                        break;
                }
            }
            if (msg != null)
            {
                _client.ReplyError(e, msg);
                _client.Log.Error("Command", msg);
            }
        }
        private void OnCommandExecuted(object sender, CommandEventArgs e)
        {
            _client.Log.Info("Command", $"{e.Command.Text} ({e.User.Name})");
        }

        private void OnLogMessage(object sender, LogMessageEventArgs e)
        {
            if (e.Message.Contains("UserUpdated") && e.Severity == LogSeverity.Error)
            {
                return;
            }

            //Color
            ConsoleColor color;
            switch (e.Severity)
            {
                case LogSeverity.Error: color = ConsoleColor.Red; break;
                case LogSeverity.Warning: color = ConsoleColor.Yellow; break;
                case LogSeverity.Info: color = ConsoleColor.White; break;
                case LogSeverity.Verbose: color = ConsoleColor.Gray; break;
                case LogSeverity.Debug: default: color = ConsoleColor.DarkGray; break;
            }

            //Exception
            string exMessage;
            Exception ex = e.Exception;
            if (ex != null)
            {
                while (ex is AggregateException && ex.InnerException != null)
                    ex = ex.InnerException;
                exMessage = ex.Message;
            }
            else
                exMessage = null;

            //Source
            string sourceName = e.Source?.ToString();

            //Text
            string text;
            if (e.Message == null)
            {
                text = exMessage ?? "";
                exMessage = null;
            }
            else
                text = e.Message;

            //Build message
            StringBuilder builder = new StringBuilder(text.Length + (sourceName?.Length ?? 0) + (exMessage?.Length ?? 0) + 5);

            builder.Append('[');
            builder.Append(DateTime.Now);
            builder.Append("] ");

            if (sourceName != null)
            {
                builder.Append('[');
                builder.Append(sourceName);
                builder.Append("] ");
            }
            for (int i = 0; i < text.Length; i++)
            {
                //Strip control chars
                char c = text[i];
                if (!char.IsControl(c))
                    builder.Append(c);
            }
            if (exMessage != null)
            {
                builder.Append(": ");
                builder.Append(exMessage);
            }

            text = builder.ToString();
            Console.ForegroundColor = color;
            Console.WriteLine(text);

            using (StreamWriter w = File.AppendText(LogFile))
            {
                w.WriteLine(text);
            }
        }

        private int PermissionResolver(User user, Channel channel)
        {
            if (user.Id == GlobalSettings.Users.DevId)
                return (int)PermissionLevel.BotOwner;
            if (user.Id == GlobalSettings.Users.RageId)
                return (int)PermissionLevel.UserPlus;
            if (user.Server != null)
            {
                if (user == channel.Server.Owner)
                    return (int)PermissionLevel.ServerOwner;

                var serverPerms = user.ServerPermissions;
                if (serverPerms.ManageRoles)
                    return (int)PermissionLevel.ServerAdmin;
                if (serverPerms.ManageMessages && serverPerms.KickMembers && serverPerms.BanMembers)
                    return (int)PermissionLevel.ServerModerator;

                var channelPerms = user.GetPermissions(channel);
                if (channelPerms.ManagePermissions)
                    return (int)PermissionLevel.ChannelAdmin;
                if (channelPerms.ManageMessages)
                    return (int)PermissionLevel.ChannelModerator;
            }
            return (int)PermissionLevel.User;
        }
    }
}
