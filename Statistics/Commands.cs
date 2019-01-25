﻿using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;
using TShockAPI.DB;
using System.IO;
using System.Reflection;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Statistics
{
    public static class Commands
    {
        public static TShockAPI.TSPlayer player;
        public static Config config = Statistics.config;
        private static TimeSpan startSpree = new TimeSpan(0, 0, 0, 30);

        public static void SpeedSpree(CommandArgs args)
        {
            config = Statistics.config;
            player = args.Player;

            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "-l":
                    SpeedKills.listTimers();
                    break;
                case "-start":
                    SpeedKills.StartSpeedKill();
                    break;
                case "-stop":
                    SpeedKills.StopSpeedKill();
                    break;
                case "-i":
                case "-interval":
                    config.SpeedSpreeTimeout = Int32.Parse(args.Parameters[1]);
                    break;
                case "-p":
                    SpeedKills.NewPlayer(Int32.Parse(args.Parameters[1]));
                    break;
            }
        }

        public static void BlitzMatch(CommandArgs args)
        {
            config = Statistics.config;
            player = args.Player;

            if (args.Parameters.Count < 1)
            {
                if (DateTime.Now >= config.BlitzEventStart)
                    config.BlitzEventStart = DateTime.Now.AddSeconds(30);

                KillingSpree.StartSpree();
                return;
            }

            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "-bt":
                case "-blitztype":
                    if (args.Parameters.Count >= 2)
                        config.BlitzEventType = args.Parameters[1];
                    break;
                case "-t":
                case "-target":
                    config.BlitzEventGoal = Int32.Parse(args.Parameters[1]);
                    break;
                case "-i":
                case "-interval":
                    config.BlitzEventLength = Int32.Parse(args.Parameters[1]);
                    break;
                case "-s":
                case "-start":
                    startSpree = new TimeSpan(0, 0, 0, 30);
                    if (args.Parameters.Count >= 2)
                        try
                        {
                            if (args.Parameters[1].Contains("h"))
                                startSpree = DateTime.ParseExact(args.Parameters[1], "H'h'm'm'", CultureInfo.InvariantCulture).TimeOfDay;
                            else
                                startSpree = DateTime.ParseExact(args.Parameters[1], "m'm'", CultureInfo.InvariantCulture).TimeOfDay;
                        }
                        catch
                        {
                            args.Player.SendErrorMessage("Start is not a valid date/time.");
                            return;
                        }
                    break;

                case "-go":
                    config.BlitzEventStart = DateTime.Now.Add(startSpree);
                    KillingSpree.StartSpree();
                    break;
            }
        }

        public static void Core(CommandArgs args)
        {
            config = Statistics.config;
            player = args.Player;
            string[] statList = { "MobKills", "BossKills", "PlayerKills" };

            if (args.Parameters.Count < 1)
            {
                string options = "";
                options += "-k(ills) - list the number of kills for the player";
                options += Environment.NewLine + "-t(ime) - show time on for player";
                options += Environment.NewLine + "-hs/highscores - list the top 5 players";
                options += Environment.NewLine + "-d(amage) - show the amount of damage";
                options += Environment.NewLine + "-s(een) - when the player was last seen";
                options += Environment.NewLine + "-l(ist) - show all the data for the player(s)";
                options += Environment.NewLine + "-stop - stops the Announcement feature";
                options += Environment.NewLine + "-prune <days> - will prune KillingSpree table of data before <days> ago";
                options += Environment.NewLine + "-o(ptions) will list all the values of the config file";
                options += Environment.NewLine + "-r(eload) will reload the current config file and set all the values and time intervals as specified";
                options += Environment.NewLine + "-n(otify) will send out all the announcements now. This will not impact the timing";

                args.Player.SendErrorMessage("Invalid syntax. /stats [flag] <player name>");
                args.Player.SendErrorMessage(options);

                return;
            }

            switch (args.Parameters[0].ToLowerInvariant())
            {
                case "-debug":
                    if (args.Player.RealPlayer)
                    {
                        args.Player.SendErrorMessage("Invalid stats option");
                        return;
                    }
                    Statistics.statsDebug = true;
                    Console.WriteLine("Debug mode set");
                    break;
                case "-kl":
                    if (args.Player.RealPlayer)
                    {
                        args.Player.SendErrorMessage("Invalid stats option");
                        return;
                    }
                    var playerData = TShock.UserAccounts.GetUserAccountByID(Int32.Parse(args.Parameters[1]));
                    var userName = TShock.UserAccounts.GetUserAccountByID(Int32.Parse(args.Parameters[1]));
                    KillingSpree.SendKillingNotice(playerData.Name, Int32.Parse(args.Parameters[1]), 1, 0, 0);
                    break;
                case "-km":
                    if (args.Player.RealPlayer)
                    {
                        args.Player.SendErrorMessage("Invalid stats option");
                        return;
                    }
                    playerData = TShock.UserAccounts.GetUserAccountByID(Int32.Parse(args.Parameters[1]));
                    if (args.Player.RealPlayer)
                    {
                        args.Player.SendErrorMessage("Invalid stats option");
                        return;
                    }
                    Statistics.database.UpdateKillingSpree(Int32.Parse(args.Parameters[1]), 1, 0, 0);
                    KillingSpree.SendKillingNotice(playerData.Name, Int32.Parse(args.Parameters[1]), 1, 0, 0);
                    break;
                case "-kb":
                    if (args.Player.RealPlayer)
                    {
                        args.Player.SendErrorMessage("Invalid stats option");
                        return;
                    }
                    playerData = TShock.UserAccounts.GetUserAccountByID(Int32.Parse(args.Parameters[1]));
                    Statistics.database.UpdateKillingSpree(Int32.Parse(args.Parameters[1]), 0, 1, 0);
                    KillingSpree.SendKillingNotice(playerData.Name, Int32.Parse(args.Parameters[1]), 0, 1, 0);
                    break;
                case "-kk":
                    SpeedKills.PlayerKill(Int32.Parse(args.Parameters[1]));
                    break;
                case "-kp":
                    if (args.Player.RealPlayer)
                    {
                        args.Player.SendErrorMessage("Invalid stats option");
                        return;
                    }
                    playerData = TShock.UserAccounts.GetUserAccountByID(Int32.Parse(args.Parameters[1]));
                    Statistics.database.UpdateKillingSpree(Int32.Parse(args.Parameters[1]), 0, 0, 1);
                    KillingSpree.SendKillingNotice(playerData.Name, Int32.Parse(args.Parameters[1]), 0, 0, 1);
                    break;
                case "-kd":
                    if (args.Player.RealPlayer)
                    {
                        args.Player.SendErrorMessage("Invalid stats option");
                        return;
                    }
                    Statistics.database.CloseKillingSpree(Int32.Parse(args.Parameters[1]));
                    KillingSpree.ClearBlitzEvent(Int32.Parse(args.Parameters[1]));
                    break;

                case "-o":
                case "-options":

                    if (args.Player.RealPlayer)
                    {
                        args.Player.SendErrorMessage("Invalid stats option");
                        return;
                    }
                    Announcements.ConsoleSendMessage(string.Format(" isActive {0}", config.isActive));
                    Announcements.ConsoleSendMessage(string.Format(" byTime {0}", config.byTime));
                    Announcements.ConsoleSendMessage(string.Format(" showTimeStamp {0}", config.showTimeStamp));
                    Announcements.ConsoleSendMessage(string.Format(" tellConsole {0}", config.tellConsole));
                    Announcements.ConsoleSendMessage(string.Format(" consoleColor {0}", config.consoleColor));

                    Announcements.ConsoleSendMessage(string.Format(" showKills {0}", config.showKills));
                    Announcements.ConsoleSendMessage(string.Format(" KillstimeInterval {0}", config.KillstimeInterval));
                    Announcements.ConsoleSendMessage(string.Format(" KillstimeOffset {0}", config.KillstimeOffset));
                    Announcements.ConsoleSendMessage(string.Format(" KillsColor {0}", string.Join(",", config.KillsColor)));

                    Announcements.ConsoleSendMessage(string.Format(" DamagetimeInterval {0}", config.DamagetimeInterval));
                    Announcements.ConsoleSendMessage(string.Format(" showDamageKills {0}", config.showDamage));
                    Announcements.ConsoleSendMessage(string.Format(" DamagetimeOffset {0}", config.DamagetimeOffset));
                    Announcements.ConsoleSendMessage(string.Format(" DamageColor {0}", string.Join(",", config.DamageColor)));

                    Announcements.ConsoleSendMessage(string.Format(" DeathstimeInterval {0}", config.DeathstimeInterval));
                    Announcements.ConsoleSendMessage(string.Format(" showDeathsKills {0}", config.showDeaths));
                    Announcements.ConsoleSendMessage(string.Format(" DeathstimeOffset {0}", config.DeathstimeOffset));
                    Announcements.ConsoleSendMessage(string.Format(" DeathsColor {0}", string.Join(",", config.DeathsColor)));

                    Announcements.ConsoleSendMessage(string.Format(" KillingSpree {0}", config.KillingSpree));
                    Announcements.ConsoleSendMessage(string.Format(" KillingSpreeThreshold {0}", string.Join(",", config.KillingSpreeThreshold)));
                    Announcements.ConsoleSendMessage(string.Format(" KillingSpreeMessage {0}", string.Join(",", config.KillingSpreeMessage)));
                    Announcements.ConsoleSendMessage(string.Format(" KillingSpreeType {0}", config.KillingSpreeType));
                    Announcements.ConsoleSendMessage(string.Format(" KillingSpreeColor {0}", string.Join(",", config.KillingSpreeColor)));

                    Announcements.ConsoleSendMessage(string.Format(" SpreeStart {0}", config.BlitzEventStart));
                    Announcements.ConsoleSendMessage(string.Format(" SpreeLength {0}", config.BlitzEventLength));
                    Announcements.ConsoleSendMessage(string.Format(" SpreeEndByTime {0}", config.BlitzEventEndByTime));
                    Announcements.ConsoleSendMessage(string.Format(" SpreeGoal {0}", config.BlitzEventGoal));
                    Announcements.ConsoleSendMessage(string.Format(" SpreeType {0}", config.BlitzEventType));

                    break;

                case "-prune":
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("No days specified for pruning.");
                        return;
                    }
                    int days = 0;
                    try
                    {
                        days = Int32.Parse(args.Parameters[1]);
                    }
                    catch
                    {
                        args.Player.SendErrorMessage("days specified is not numeric.");
                        return;
                    }
                    if (days <= 0 || days >= 1000)
                    {
                        args.Player.SendErrorMessage("days must be > 0 and < 1000.");
                        return;
                    }
                    int[] counts = Statistics.database.PruneKillingSpree(days);
                    Announcements.ConsoleSendMessage(string.Format(" KillingSpree pruned {0} days: before[{1}] after[{2}].", Int32.Parse(args.Parameters[1]), counts[0], counts[1]));

                    break;

                case "-r":
                case "-reload":
                    if (args.Player.RealPlayer)
                    {
                        args.Player.SendErrorMessage("Invalid stats option");
                        return;
                    }
                    Statistics.config = Config.loadConfig(Statistics.configPath);
                    Announcements.stopAnnouncements();
                    Announcements.setupAnnouncements();
                    Announcements.ConsoleSendMessage(string.Format(" Announcements config reloaded"));

                    break;
                case "-stop":
                    Statistics.config.isActive = false;
                    Announcements.stopAnnouncements();
                    Announcements.ConsoleSendMessage(string.Format(" Announcements stopped"));

                    break;

//                case "-init":
//                    Statistics.database.dropTables();
//                    Statistics.OnInitialize(null);
//                    break;

                case "-n":
                case "-notify":
                    if (args.Player.RealPlayer)
                    {
                        args.Player.SendErrorMessage("Invalid stats option");
                        return;
                    }
                    if (args.Parameters.Count < 2)
                        Announcements.SendNoticeAll("");
                    else
                    {
                        if (statList.Contains(args.Parameters[1]))
                            Announcements.SendNoticeAll(args.Parameters[1]);
                        else
                            Announcements.ConsoleSendMessage(string.Format(" Invalid stats option"));
                    }
                    break;

                case "-l":
                case "-list":
                    {
                        Dictionary<string, int[]> statsList = new Dictionary<string, int[]>();
                        int page = 0, totalPages = 0;

                        totalPages = Statistics.database.CountAllPlayers();
                        if (totalPages == 0)
                        {
                            args.Player.SendErrorMessage("No statistical data available");
                            return;
                        }

                        var lineColor = Color.Yellow;
                        totalPages = (totalPages / 5) + 1;
                        statsList.Clear();
                        if (args.Parameters.Count < 2)
                        {
                            statsList = Statistics.database.GetAllPlayers(page, 0);
                            page = 1;
                        }
                        else
                        {
                            bool isNum = Int32.TryParse(args.Parameters[1], out page);
                            if (isNum)
                            {
                                statsList = Statistics.database.GetAllPlayers(page, 0);

                                if (!args.Player.RealPlayer)
                                    args.Player.SendInfoMessage("Statistics List - Page {0} of {1}", page, totalPages);
                                else
                                    args.Player.SendMessage(string.Format("Statistics List - Page {0} of {1}", page, totalPages), lineColor);
                            }
                            else
                            {
                                var user = TShock.UserAccounts.GetUserAccounts().Find(u => u.Name.StartsWith(args.Parameters[1]));

                                if (user == null)
                                    args.Player.SendErrorMessage("No users found matching the name '{0}'", args.Parameters[1]);
                                else
                                    statsList = Statistics.database.GetAllPlayers(1, user.ID);
                            }
                        }
                        foreach (KeyValuePair<string, int[]> stat in statsList)
                        {
                            string statsName = stat.Key;
                            int[] stats = stat.Value;
                            TimeSpan ts = new TimeSpan(0, 0, 0, stats[2]);
                            var total = ts.Add(new TimeSpan(0, 0, 0, Statistics.TimeCache[stats[0]]));
                            if (!args.Player.RealPlayer)
                                args.Player.SendInfoMessage(" {0}, died {1} kills: player {4} mob {5} boss {6} - damage: mob {7} boss {8} player {9} received {10} on for {2} logins {3}",
        statsName, stats[1], total.SToString(), stats[3], stats[4], stats[5], stats[6], stats[7], stats[8], stats[9], stats[10]);
                            else
                                args.Player.SendMessage(string.Format(" {0}, died {1} kills: player {4} mob {5} boss {6} - damage: mob {7} boss {8} player {9} received {10} on for {2} logins {3}",
        statsName, stats[1], total.SToString(), stats[3], stats[4], stats[5], stats[6], stats[7], stats[8], stats[9], stats[10]), lineColor);
                        }
                    }
                    break;
                case "-k":
                case "-kills":
                    {
                        if (args.Parameters.Count < 2)
                        {
                            var kills = Statistics.database.GetCurrentKills(args.Player.Account.ID);
                            if (kills == null)
                                args.Player.SendErrorMessage("Unable to discover your killcount. Sorry.");
                            else
                                args.Player.SendSuccessMessage(
                                    "You have killed {0} player{4}, {1} mob{5}, {2} boss{6} and died {3} time{7}",
                                    kills[4], kills[5], kills[6], kills[3],
                                    kills[4].Suffix(), kills[5].Suffix(), kills[6].Suffix(true), kills[3].Suffix());
                        }
                        else
                        {
                            var name = args.Parameters[1];
                            var user = TShock.UserAccounts.GetUserAccounts().Find(
                              u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                              );
                            if (user == null)
                                args.Player.SendErrorMessage("No users found matching the name '{0}'", name);
                            else
                            {
                                var kills = Statistics.database.GetCurrentKills(user.ID);
                                if (kills == null)
                                    args.Player.SendErrorMessage("Unable to discover the killcount of {0}. Sorry.",
                                        user.Name);
                                else
                                {
                                    args.Player.SendSuccessMessage(
                                        "{0} has killed {1} player{5}, {2} mob{6}, {3} boss{7} and died {4} time{8}",
                                        user.Name, kills[4], kills[5], kills[6], kills[3],
                                        kills[4].Suffix(), kills[5].Suffix(), kills[6].Suffix(true), kills[3].Suffix());
                                }
                            }
                        }
                        break;
                    }
                case "-t":
                case "-time":
                    {
                        var logins = 1;
                        if (args.Parameters.Count < 2)
                        {
                            var times = Statistics.database.GetTimes(args.Player.Account.ID, ref logins);
                            if (times == null)
                                args.Player.SendErrorMessage("Unable to discover your times. Sorry.");
                            else
                            {
                                var total = times[1].Add(new TimeSpan(0, 0, 0, Statistics.TimeCache[args.Player.Index]));
                                args.Player.SendSuccessMessage("You have played for {0}.", total.SToString());
                                args.Player.SendSuccessMessage("You have been registered for {0}.", times[0].SToString());
                                args.Player.SendSuccessMessage("You have logged in {0} times.", logins);
                            }
                        }
                        else
                        {
                            var name = args.Parameters[1];
                            var users = GetUsers(name);
                            if (users.Count > 1)
                            {
                                args.Player.SendErrorMessage("More than one user matched your search '{0}': {1}",
                                    name, string.Join(", ", users.Select(u => u.Name)));
                                break;
                            }
                            if (users.Count == 0)
                            {
                                args.Player.SendErrorMessage("No users matched your search '{0}'", name);
                                break;
                            }

                            var user = users[0];

                            var times = Statistics.database.GetTimes(user.ID, ref logins);
                            if (times == null)
                                args.Player.SendErrorMessage("Unable to discover the times of {0}. Sorry.",
                                    user.Name);
                            else
                            {
                                args.Player.SendSuccessMessage("{0} has played for {1}.", user.Name,
                                    times[1].SToString());
                                args.Player.SendSuccessMessage("{0} has been registered for {1}.", user.Name,
                                    times[0].SToString());
                                args.Player.SendSuccessMessage("{0} has logged in {1} times.", user.Name, logins);
                            }
                        }
                        break;
                    }
                case "-s":
                case "-seen":
                    {
                        if (args.Parameters.Count < 2)
                            args.Player.SendErrorMessage("Invalid syntax. /stats -s [player name]");
                        else
                        {
                            var name = args.Parameters[1];
                            var users = GetUsers(name);
                            if (users.Count > 1)
                            {
                                args.Player.SendErrorMessage("More than one user matched your search '{0}': {1}",
                                    name, string.Join(", ", users.Select(u => u.Name)));
                                break;
                            }
                            if (users.Count == 0)
                            {
                                args.Player.SendErrorMessage("No users matched your search '{0}'", name);
                                break;
                            }

                            var user = users[0];
                            var seen = Statistics.database.GetLastSeen(user.ID);
                            if (seen == TimeSpan.MaxValue)
                                args.Player.SendErrorMessage("Unable to find {0}'s last login time.",
                                    user.Name);
                            else
                                args.Player.SendSuccessMessage("{0} last logged in {1} ago.", user.Name, seen.SToString());
                        }

                        break;
                    }
                case "-hs":
                case "-highscores":
                    {
                        var highscores = new Dictionary<string, int>();
                        var page = 1;
                        if (args.Parameters.Count < 2)
                            highscores = Statistics.database.GetHighScores(1);
                        else if (HsPagination.TryParsePageNumber(args.Parameters, 1, args.Player, out page))
                            highscores = Statistics.database.GetHighScores(page);

                        HsPagination.SendPage(args.Player, page, highscores, new HsPagination.FormatSettings
                        {
                            FooterFormat = "use /stats -hs {0} for more high scores",
                            FooterTextColor = Color.Lime,
                            HeaderFormat = "High Scores- Page {0} of {1}",
                            HeaderTextColor = Color.Lime,
                            IncludeFooter = true,
                            IncludeHeader = true,
                            MaxLinesPerPage = 5,
                            NothingToDisplayString = "No highscores available"
                        });

                        break;
                    }
                case "-d":
                case "-damage":
                    {
                        if (args.Parameters.Count < 2)
                        {
                            var damages = Statistics.database.GetDamage(args.Player.Account.ID);
                            if (damages == null)
                            {
                                args.Player.SendErrorMessage("Unable to discover your damage statistics. Sorry.");
                                return;
                            }
                            args.Player.SendSuccessMessage("You have dealt {0} damage to mobs, {1} damage to bosses "
                                                           + "and {2} damage to players.", damages[0], damages[1], damages[2]);
                            args.Player.SendSuccessMessage("You have been dealt {0} damage.", damages[3]);
                        }
                        else
                        {
                            var name = args.Parameters[1];
                            var users = GetUsers(name);
                            if (users.Count > 1)
                            {
                                args.Player.SendErrorMessage("More than one user matched your search '{0}': {1}",
                                    name, string.Join(", ", users.Select(u => u.Name)));
                                break;
                            }
                            if (users.Count == 0)
                            {
                                args.Player.SendErrorMessage("No users matched your search '{0}'", name);
                                break;
                            }

                            var user = users[0];
                            var damages = Statistics.database.GetDamage(user.ID);
                            if (damages == null)
                            {
                                args.Player.SendErrorMessage("Unable to discover your damage statistics. Sorry.");
                                return;
                            }
                            args.Player.SendSuccessMessage("{0} has dealt {1} damage to mobs, {2} damage to bosses "
                                + "and {3} damage to players.", user.Name, damages[0], damages[1], damages[2]);
                            args.Player.SendSuccessMessage("{0} has been dealt {1} damage.", user.Name, damages[3]);
                        }
                        break;
                    }
                case "-ix":
                case "-infox":
                    {
                        break;
                    }
            }
        }

        private static List<UserAccount> GetUsers(string username)
        {
            var users = TShock.UserAccounts.GetUserAccounts();
            var ret = new List<UserAccount>();
            foreach (var user in users)
            {
                if (user.Name.Equals(username))
                    return new List<UserAccount> { user };
                if (user.Name.StartsWith(username))
                    ret.Add(user);
            }
            return ret;
        }
    }
}