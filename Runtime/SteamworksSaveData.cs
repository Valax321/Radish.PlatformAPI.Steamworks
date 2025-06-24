using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;

namespace Radish.PlatformAPI.Steamworks
{
    internal sealed class SteamworksSaveData : IPlatformSaveData
    {
        public bool isSupported => SteamClient.IsValid;

        public event IPlatformSaveData.SaveDataDynamicChangeDelegate OnSaveDataChanged;

        public SteamworksSaveData()
        {
            SteamRemoteStorage.OnLocalFileChange += OnFilesChanged;
        }

        private void OnFilesChanged()
        {
            var changes = new List<IPlatformSaveData.FileChange>();
            var changeCount = SteamRemoteStorage.GetLocalFileChangeCount();
            for (var i = 0; i < changeCount; ++i)
            {
                var path = SteamRemoteStorage.GetLocalFileChange(i, out var change, out var type);
                
                // We don't use autocloud via this API, so skip those changes
                if (type == RemoteStorageFilePathType.Absolute)
                    continue;
                
                changes.Add(new IPlatformSaveData.FileChange(path, change switch
                {
                    RemoteStorageLocalFileChange.FileUpdated => IPlatformSaveData.ChangeType.Updated,
                    RemoteStorageLocalFileChange.FileDeleted => IPlatformSaveData.ChangeType.Deleted,
                    _ => throw new ArgumentOutOfRangeException()
                }));
            }
            
            OnSaveDataChanged?.Invoke(changes);
        }

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