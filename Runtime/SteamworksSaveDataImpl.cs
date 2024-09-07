using System;
using System.Collections.Generic;
using System.IO;
using Radish.PlatformAPI.DefaultAPIs;
using Steamworks;
using UnityEngine;

namespace Radish.PlatformAPI.Steamworks
{
    internal sealed class SteamworksSaveDataImpl : IPlatformSaveData
    {
        private readonly ulong m_SteamUserId = SteamClient.SteamId.Value;
        private readonly PlatformSaveDataImplFileIO m_FileIO;

        public SteamworksSaveDataImpl()
        {
            m_FileIO = new PlatformSaveDataImplFileIO(Application.persistentDataPath, "system",
                m_SteamUserId.ToString());
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

        public Stream OpenUserDataStream(string name, IPlatformSaveData.OpenMode mode)
        {
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
            return SteamRemoteStorage.FileExists(name);
        }

        public void DeleteUserData(string name)
        {
            SteamRemoteStorage.FileDelete(name);
        }

        public IEnumerable<string> GetUserDataNames()
        {
            return SteamRemoteStorage.Files;
        }
    }
}