using Steamworks;

namespace Radish.PlatformAPI.Steamworks
{
    internal sealed class SteamworksUserInfo : IPlatformUserInfo
    {
        public string localUserDisplayName => SteamClient.Name;
    }
}