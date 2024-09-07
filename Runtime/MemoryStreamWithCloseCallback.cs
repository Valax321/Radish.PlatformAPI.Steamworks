using System;
using System.IO;

namespace Radish.PlatformAPI.Steamworks
{
    internal sealed class MemoryStreamWithCloseCallback : MemoryStream
    {
        public override bool CanWrite => base.CanWrite && !ForceDisableWrite;

        public event Action OnClosed
        {
            add => m_OnClosed += value;
            remove => m_OnClosed -= value;
        }
        
        public bool ForceDisableWrite { get; set; }

        private Action m_OnClosed;
        
        public override void Close()
        {
            base.Close();
            m_OnClosed?.Invoke();
        }
    }
}