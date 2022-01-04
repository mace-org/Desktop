using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Desktop.Infrastructures
{
    public enum KeyboardInputType
    {
        KeyDown = 0x0100,
        KeyUp = 0x0101,
        SysKeyDown = 0x0104,
        SysKeyUp = 0x0105
    }

    public record KeyboardInput(KeyboardInputType Type, Keys VkCode, uint ScanCode, uint Time);

    public class KeyboardHook : WindowsHook<KeyboardInput>
    {
        public KeyboardHook() : base(NativeApi.WH_KEYBOARD_LL, "low level keyboard")
        {
        }

        protected override KeyboardInput CreateMessage(int wParam, IntPtr lParam)
        {
            var input = Marshal.PtrToStructure<NativeApi.KBDLLHOOKSTRUCT>(lParam);
            var args = new KeyboardInput((KeyboardInputType)wParam, (Keys)input.vkCode, input.scanCode, input.time);
            return args;
        }

    }
}
