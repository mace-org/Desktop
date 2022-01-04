using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
    public enum MouseInputType
    {
        MouseMove = 0x0200,
        LeftButtonDown = 0x0201,
        LeftButtonUp = 0x0202,
        RightButtonDown = 0x0204,
        RightButtonUp = 0x0205,
        MouseWheel = 0x020A,
        MouseHWheel = 0x020E
    }

    public record MouseInput(MouseInputType Type, Point Position, uint MouseData, uint Time);

    public class MouseHook : WindowsHook<MouseInput>
    {
        public MouseHook() : base(NativeApi.WH_MOUSE_LL, "low level mouse")
        {
        }

        protected override MouseInput CreateMessage(int wParam, IntPtr lParam)
        {
            var input = Marshal.PtrToStructure<NativeApi.MSLLHOOKSTRUCT>(lParam);
            var args = new MouseInput((MouseInputType)wParam, new Point(input.pt.x, input.pt.y), input.mouseData, input.time);
            return args;
        }
    }
}
