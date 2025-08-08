using System.Linq;

namespace PlaytimeCounter
{
    public static class Extensions
    {
        //GROUP EXTENSIONS

        public static string GetGroupKey(this UserGroup group)
        {
            if (group == null)
                return "default";

            if (!ServerStatic.PermissionsHandler.Groups.Values.Any(x => x.IsGroupEqual(group)))
                return "default";

            return ServerStatic.PermissionsHandler.Groups.FirstOrDefault(pair => IsGroupEqual(pair.Value, group)).Key;
        }

        public static bool IsGroupEqual(this UserGroup @this, UserGroup other)
            => (@this.BadgeColor == other.BadgeColor)
           && (@this.Name == other.Name)
           && (@this.BadgeText == other.BadgeText)
           && (@this.Permissions == other.Permissions)
           && (@this.Cover == other.Cover)
           && (@this.HiddenByDefault == other.HiddenByDefault)
           && (@this.Shared == other.Shared)
           && (@this.KickPower == other.KickPower)
           && (@this.RequiredKickPower == other.RequiredKickPower);

        public static UserGroup GetGroup(this string groupKey)
        {
            ServerStatic.PermissionsHandler.GetAllGroups().TryGetValue(groupKey, out UserGroup userGroup);
            return userGroup;
        }
    }
}
