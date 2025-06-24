using Steamworks;

namespace Radish.PlatformAPI.Steamworks
{
    internal sealed class SteamworksUserInfo : IPlatformUserInfo
    {
        public string localUserDisplayName => isSupported ? SteamClient.Name : string.Empty;
        public bool isSupported => SteamClient.IsValid;
    }
}