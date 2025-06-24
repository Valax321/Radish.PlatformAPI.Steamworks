using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;

namespace Radish.PlatformAPI.Steamworks
{
    internal sealed class SteamworksSaveData : IPlatformSaveData
    {
        public bool isSupported => SteamClient.IsValid;
        
        public bool BeginDataWrite()
        {
            return SteamRemoteStorage.BeginFileWriteBatch();
        }

        public bool EndDataWrite()
        {
            return SteamRemoteStorage.EndFileWriteBatch();
        }

        public Stream OpenDataStream(string name, IPlatformSaveData.OpenMode mode)
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

        public bool DoesDataExist(string name)
        {
            if (!isSupported)
                return false;
            
            return SteamRemoteStorage.FileExists(name);
        }

        public void DeleteData(string name)
        {
            if (!isSupported)
                return;
            
            SteamRemoteStorage.FileDelete(name);
        }

        public IEnumerable<string> GetDataNames()
        {
            if (!isSupported)
                return Array.Empty<string>();
            
            return SteamRemoteStorage.Files;
        }
    }
}