using MEC;
using PlaytimeCounter.Features;
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
        public static event EventHandler<RoundEndEvent> RoundEndEvent;

        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerJoined)]
        public void OnPlayerJoined(PlayerJoinedEvent ev)
        {
            PlayerJoinedEvent?.Invoke(this, ev);
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.PlayerLeft)]
        public void OnPlayerLeft(PlayerLeftEvent ev)
        {
            PlayerLeftEvent?.Invoke(this, ev);

            //Leaving the game will not save playtime, therefore we need to fire this event.
            PlayerChangeRoleEvent roleEv = new PlayerChangeRoleEvent(ev.Player.ReferenceHub, ev.Player.RoleBase, PlayerRoles.RoleTypeId.None, PlayerRoles.RoleChangeReason.Destroyed);
            PlayerChangeRoleEvent?.Invoke(this, roleEv);
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

        [PluginEvent(PluginAPI.Enums.ServerEventType.RoundEnd)]
        public void OnRoundEnd(RoundEndEvent ev)
        {
            RoundEndEvent?.Invoke(this, ev);
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

            if(!Timing.IsRunning(SummaryTimer.summaryHandle))
            {
                SummaryTimer.summaryHandle = Timing.RunCoroutine(SummaryTimer.SummaryTimerCheck());
            }
        }
    }
}
