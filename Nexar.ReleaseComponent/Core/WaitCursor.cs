using System;
using System.Windows.Input;

namespace Nexar.ReleaseComponent
{
    public class WaitCursor : IDisposable
    {
        readonly Cursor _lastCursor;

        public WaitCursor()
        {
            _lastCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
        }

        public void Dispose()
        {
            Mouse.OverrideCursor = _lastCursor;
        }
    }
}
