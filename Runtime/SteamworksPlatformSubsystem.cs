using System;
using JetBrains.Annotations;
using Radish.Logging;
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

        public IPlatformSaveData saveData { get; private set; }

        public IPlatformUserInfo userInfo { get; private set; }

        private readonly AppId m_AppId;
        private readonly bool m_RestartIfNecessary;

        public SteamworksPlatformSubsystem(uint appId, bool restartIfNecessary)
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
                Logger.Info("Initialized steam successfully. AppId: {0}", m_AppId);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to initialize steam client: {0}", ex.Message);
            }
            
            saveData = new SteamworksSaveData();
            userInfo = new SteamworksUserInfo();
            
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