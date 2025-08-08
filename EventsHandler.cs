using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using MEC;
using PlaytimeCounter.Features;
using PlaytimeCounter.Features.Discord;
using System;

namespace PlaytimeCounter
{
    public class EventsHandler
    {
        public static event Action<PlayerJoinedEventArgs> PlayerJoinedEvent;
        public static event Action<PlayerLeftEventArgs> PlayerLeftEvent;
        public static event Action<PlayerChangingRoleEventArgs> PlayerChangeRoleEvent;
        public static event Action RoundStartEvent;
        public static event Action<RoundEndedEventArgs> RoundEndEvent;

        public void OnPlayerJoined(PlayerJoinedEventArgs ev)
        {
            PlayerJoinedEvent?.Invoke(ev);
        }

        public void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            PlayerLeftEvent?.Invoke(ev);

            if (!CustomNetworkManager.TypedSingleton._disconnectDrop)
            {
                //Leaving the game will not save playtime, therefore we need to fire this event.
                PlayerChangingRoleEventArgs roleEv = new PlayerChangingRoleEventArgs(ev.Player.ReferenceHub, ev.Player.RoleBase, PlayerRoles.RoleTypeId.None, PlayerRoles.RoleChangeReason.Destroyed, PlayerRoles.RoleSpawnFlags.All);
                PlayerChangeRoleEvent?.Invoke(roleEv);
            }
        }

        public void OnPlayerChangeRole(PlayerChangingRoleEventArgs ev)
        {
            PlayerChangeRoleEvent?.Invoke(ev);
        }

        public void OnRoundStart()
        {
            RoundStartEvent?.Invoke();
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            RoundEndEvent?.Invoke(ev);
        }
    
        public void OnWaitingForPlayers()
        {
            if (!Timing.IsRunning(DiscordWebhookHandler.msgHandle))
            {
                DiscordWebhookHandler.msgHandle = Timing.RunCoroutine(DiscordWebhookHandler.MessageQueueCoroutine());
            }

            if (!Timing.IsRunning(DiscordWebhookHandler.queueHandle))
            {
                DiscordWebhookHandler.queueHandle = Timing.RunCoroutine(DiscordWebhookHandler.WebhookQueueCoroutine());
            }

            if (!Timing.IsRunning(SummaryTimer.summaryHandle))
            {
                SummaryTimer.summaryHandle = Timing.RunCoroutine(SummaryTimer.SummaryTimerCheck());
            }
        }
    }
}
