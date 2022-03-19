using Exiled.API.Features;
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
        public override Version Version => new Version(1, 0, 1);
        public override Version RequiredExiledVersion => new Version(4, 2, 5);

        public Dictionary<string, long> joinTime = new Dictionary<string, long>();

        public override void OnEnabled()
        {
            instance = this;
            eventsHandler = new EventsHandler();

            Exiled.Events.Handlers.Player.Verified += eventsHandler.OnVerified;
            Exiled.Events.Handlers.Player.Destroying += eventsHandler.OnDestroyed;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Verified -= eventsHandler.OnVerified;
            Exiled.Events.Handlers.Player.Destroying -= eventsHandler.OnDestroyed;

            eventsHandler = null;
            instance = null;

            base.OnDisabled();
        }
    }
}
