﻿using PluginAPI;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaytimeCounter
{
    public static class Extensions
    {
        //GROUP EXTENSIONS

        public static UserGroup GetGroupFromString(this string groupName)
        {
            Log.Debug($"{ServerStatic.GetPermissionsHandler() == null}");
            if(!ServerStatic.GetPermissionsHandler().GetAllGroups().TryGetValue(groupName, out UserGroup userGroup))
            {
                Log.Error($"Failed getting group as it doesn't exist!");
            }
            return userGroup;
        }

        public static string GetGroupKey(this UserGroup group)
        {
            return ServerStatic.PermissionsHandler._groups.FirstOrDefault(pair => pair.Value.IsGroupEqual(group)).Key;
        }

        public static bool IsGroupEqual(this UserGroup group, UserGroup other)
            => (group.BadgeColor == other.BadgeColor)
            && (group.BadgeText == other.BadgeText)
            && (group.Permissions == other.Permissions)
            && (group.Cover == other.Cover)
            && (group.HiddenByDefault == other.HiddenByDefault)
            && (group.Shared == other.Shared)
            && (group.KickPower == other.KickPower)
            && (group.RequiredKickPower == other.RequiredKickPower);
    }
}
