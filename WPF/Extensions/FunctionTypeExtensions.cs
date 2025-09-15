using System;
using Helper;
using Persistence.Models;
using SharpDX.DirectInput;

namespace Shell.WPF.Extensions
{
  public static class FunctionTypeExtensions
  {
    public static void SetJoyStick(this FunctionType e, JoystickOffset? joystick)
    {
      Configuration.Set(Enum.GetName(e)!, joystick is null ? null! : Enum.GetName((JoystickOffset)joystick)!);
    }
  }
}