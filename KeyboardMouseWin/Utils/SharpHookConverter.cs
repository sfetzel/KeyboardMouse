using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KeyboardMouseWin.Utils
{
    public static class SharpHookConverter
    {
        public static Key ToKey(this KeyCode keyCode)
            => keyCode switch
            {
                KeyCode.VcUndefined => Key.None,
                KeyCode.VcBackspace => Key.Back,
                KeyCode.VcTab => Key.Tab,
                KeyCode.VcEnter => Key.Enter,
                KeyCode.VcSpace => Key.Space,
                KeyCode.Vc0 => Key.D0,
                KeyCode.Vc1 => Key.D1,
                KeyCode.Vc2 => Key.D2,
                KeyCode.Vc3 => Key.D3,
                KeyCode.Vc4 => Key.D4,
                KeyCode.Vc5 => Key.D5,
                KeyCode.Vc6 => Key.D6,
                KeyCode.Vc7 => Key.D7,
                KeyCode.Vc8 => Key.D8,
                KeyCode.Vc9 => Key.D9,
                KeyCode.VcA => Key.A,
                KeyCode.VcB => Key.B,
                KeyCode.VcC => Key.C,
                KeyCode.VcD => Key.D,
                KeyCode.VcE => Key.E,
                KeyCode.VcF => Key.F,
                KeyCode.VcG => Key.G,
                KeyCode.VcH => Key.H,
                KeyCode.VcI => Key.I,
                KeyCode.VcJ => Key.J,
                KeyCode.VcK => Key.K,
                KeyCode.VcL => Key.L,
                KeyCode.VcM => Key.M,
                KeyCode.VcN => Key.N,
                KeyCode.VcO => Key.O,
                KeyCode.VcP => Key.P,
                KeyCode.VcQ => Key.Q,
                KeyCode.VcR => Key.R,
                KeyCode.VcS => Key.S,
                KeyCode.VcT => Key.T,
                KeyCode.VcU => Key.U,
                KeyCode.VcV => Key.V,
                KeyCode.VcW => Key.W,
                KeyCode.VcX => Key.X,
                KeyCode.VcY => Key.Y,
                KeyCode.VcZ => Key.Z,
                KeyCode.VcLeftShift => Key.LeftShift,
                KeyCode.VcRightShift => Key.RightShift,
                KeyCode.VcLeftControl => Key.LeftCtrl,
                KeyCode.VcRightControl => Key.RightCtrl,
                KeyCode.VcLeftAlt => Key.LeftAlt,
                KeyCode.VcRightAlt => Key.RightAlt,
                KeyCode.VcCapsLock => Key.CapsLock,
                KeyCode.VcF1 => Key.F1,
                KeyCode.VcF2 => Key.F2,
                KeyCode.VcF3 => Key.F3,
                KeyCode.VcF4 => Key.F4,
                KeyCode.VcF5 => Key.F5,
                KeyCode.VcF6 => Key.F6,
                KeyCode.VcF7 => Key.F7,
                KeyCode.VcF8 => Key.F8,
                KeyCode.VcF9 => Key.F9,
                KeyCode.VcF10 => Key.F10,
                KeyCode.VcF11 => Key.F11,
                KeyCode.VcF12 => Key.F12,
                KeyCode.VcComma => Key.OemComma,
                KeyCode.VcUp => Key.Up,
                KeyCode.VcDown => Key.Down,
                KeyCode.VcLeft => Key.Left,
                KeyCode.VcRight => Key.Right,
                KeyCode.VcMinus => Key.OemMinus,
                KeyCode.VcInsert => Key.Insert,
                KeyCode.VcHome => Key.Home,
                KeyCode.VcPageUp => Key.PageUp,
                KeyCode.VcPageDown => Key.PageDown,
                KeyCode.VcEscape => Key.Escape,
                _ => Key.None
            };

    }
}
