using MEC;
using PlaytimeCounter.Features.Discord;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter
{
    public class EventsHandler
    {
        public static event EventHandler<PlayerJoinedEvent> PlayerJoinedEvent;
        public static event EventHandler<PlayerLeftEvent> PlayerLeftEvent;
        public static event EventHandler<PlayerChangeRoleEvent> PlayerChangeRoleEvent;
        public static event EventHandler<RoundStartEvent> RoundStartEvent;

        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerJoined)]
        public void OnPlayerJoined(PlayerJoinedEvent ev)
        {
            PlayerJoinedEvent?.Invoke(this, ev);
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerLeft)]
        public void OnPlayerLeft(PlayerLeftEvent ev)
        {
            PlayerLeftEvent?.Invoke(this, ev);
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerChangeRole)]
        public void OnPlayerChangeRole(PlayerChangeRoleEvent ev)
        {
            PlayerChangeRoleEvent?.Invoke(this, ev);
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.RoundStart)]
        public void OnRoundStart(RoundStartEvent ev)
        {
            RoundStartEvent?.Invoke(this, ev);
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.WaitingForPlayers)]
        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            if(!Timing.IsRunning(DiscordWebhookHandler.msgHandle))
            {
                DiscordWebhookHandler.msgHandle = Timing.RunCoroutine(DiscordWebhookHandler.MessageQueueCoroutine());
            }

            if(!Timing.IsRunning(DiscordWebhookHandler.queueHandle))
            {
                DiscordWebhookHandler.queueHandle = Timing.RunCoroutine(DiscordWebhookHandler.WebhookQueueCoroutine());
            }
        }
    }
}
