using System;
using JetBrains.Annotations;
using Radish.Logging;
using Radish.PlatformAPI.DefaultAPIs;
using Steamworks;
using UnityEngine;
using ILogger = Radish.Logging.ILogger;

namespace Radish.PlatformAPI.Steamworks
{
    [PublicAPI]
    public sealed class SteamworksPlatformSubsystem : IPlatformSubsystem
    {
        private static readonly ILogger Logger = LogManager.GetLoggerForType(typeof(SteamworksPlatformSubsystem));
        
        public string name => nameof(SteamworksPlatformSubsystem);
        
        public OptionalAPI<IPlatformSaveData> saveData { get; private set; }
            = OptionalAPI<IPlatformSaveData>.CreateNotSupported("Steam API not initialized yet");
        
        public OptionalAPI<IPlatformUserInfo> userInfo { get; }
            = OptionalAPI<IPlatformUserInfo>.CreateForImplementation(new SteamworksUserInfo());

        private readonly AppId m_AppId;
        private readonly bool m_RestartIfNecessary;

        public SteamworksPlatformSubsystem(AppId appId, bool restartIfNecessary)
        {
            m_AppId = appId;
            m_RestartIfNecessary = restartIfNecessary;
        }
        
        public bool Initialize()
        {
            if (m_RestartIfNecessary && SteamClient.RestartAppIfNecessary(m_AppId.Value))
                return false;
            
            Application.quitting += OnQuit;

            try
            {
                SteamClient.Init(m_AppId.Value);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "Steam got a little angry, probably ok though :)");
            }
            
            saveData = OptionalAPI<IPlatformSaveData>.CreateForImplementation(
                new PlatformSaveDataImplFileIO(
                    Application.persistentDataPath, 
                    "system", 
                    SteamClient.SteamId.Value.ToString()
                    ));
            return true;
        }

        private void OnQuit()
        {
            SteamClient.Shutdown();
        }

        public void Update()
        {
        }
    }
}