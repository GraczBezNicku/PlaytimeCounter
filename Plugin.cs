using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.IO;

namespace PlaytimeCounter
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin instance;
        public EventsHandler eventsHandler;

        public override string Name => "PlaytimeCounter";
        public override string Author => "GBN";
        public override Version Version => new Version(2, 0, 2);
        public override Version RequiredExiledVersion => new Version(5, 2, 1);

        public Dictionary<string, long> joinTime = new Dictionary<string, long>();
        public static string GroupDir = Path.Combine(Paths.Configs, "PlaytimeCounter");

        public List<CoroutineHandle> coroutines = new List<CoroutineHandle>();

        public override void OnEnabled()
        {
            instance = this;
            eventsHandler = new EventsHandler();

            if (!Directory.Exists(GroupDir))
            {
                Log.Warn("Directory where play times are stored does not exist. Don't worry though, we're creating one right now ;)");
                Directory.CreateDirectory(GroupDir);
            }

            Exiled.Events.Handlers.Player.Verified += eventsHandler.OnVerified;
            Exiled.Events.Handlers.Player.Destroying += eventsHandler.OnDestroyed;
            Exiled.Events.Handlers.Server.WaitingForPlayers += eventsHandler.OnWaitingForPlayers;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= eventsHandler.OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.Verified -= eventsHandler.OnVerified;
            Exiled.Events.Handlers.Player.Destroying -= eventsHandler.OnDestroyed;

            coroutines.Clear();

            eventsHandler = null;
            instance = null;

            base.OnDisabled();
        }
    }
}
