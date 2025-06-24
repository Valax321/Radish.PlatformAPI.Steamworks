using System;
using System.Collections.Generic;
using System.IO;
using Radish.PlatformAPI.DefaultAPIs;
using Steamworks;
using UnityEngine;

namespace Radish.PlatformAPI.Steamworks
{
    internal sealed class SteamworksSaveData : IPlatformSaveData
    {
        public bool isSupported => SteamClient.IsValid;

        private readonly ulong m_SteamUserId;
        private readonly PlatformSaveDataImplFileIO m_FileIO;

        public SteamworksSaveData()
        {
            m_FileIO = new PlatformSaveDataImplFileIO(Application.persistentDataPath, "local",
                m_SteamUserId.ToString());

            if (!SteamClient.IsValid)
                return;
            
            m_SteamUserId = SteamClient.SteamId.Value;
        }

        public Stream OpenLocalDataStream(string name, IPlatformSaveData.OpenMode mode)
        {
            return m_FileIO.OpenUserDataStream(name, mode);
        }

        public bool DoesLocalDataExist(string name)
        {
            return m_FileIO.DoesUserDataExist(name);
        }

        public void DeleteLocalData(string name)
        {
            m_FileIO.DeleteUserData(name);
        }

        public IEnumerable<string> GetLocalDataNames()
        {
            return m_FileIO.GetUserDataNames();
        }

        public void BeginUserDataWrite()
        {
            
        }

        public void EndUserDataWrite()
        {
            throw new NotImplementedException();
        }

        public Stream OpenUserDataStream(string name, IPlatformSaveData.OpenMode mode)
        {
            if (!isSupported)
                return null;
            
            var stream = new MemoryStreamWithCloseCallback();
            if (mode.HasFlagT(IPlatformSaveData.OpenMode.Read) && SteamRemoteStorage.FileExists(name))
            {
                var data = SteamRemoteStorage.FileRead(name);
                stream.Write(data);
            }

            if (mode.HasFlagT(IPlatformSaveData.OpenMode.Write))
            {
                stream.OnClosed += () =>
                {
                    SteamRemoteStorage.FileWrite(name, stream.ToArray());
                };
            }
            else
            {
                stream.ForceDisableWrite = true;
            }

            return stream;
        }

        public bool DoesUserDataExist(string name)
        {
            if (!isSupported)
                return false;
            
            return SteamRemoteStorage.FileExists(name);
        }

        public void DeleteUserData(string name)
        {
            if (!isSupported)
                return;
            
            SteamRemoteStorage.FileDelete(name);
        }

        public IEnumerable<string> GetUserDataNames()
        {
            if (!isSupported)
                return Array.Empty<string>();
            
            return SteamRemoteStorage.Files;
        }
    }
}