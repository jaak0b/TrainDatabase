using Helper;
using Persistence;
using SharpDX.DirectInput;
using System;
using Persistence.Models;

namespace TrainDatabase.Extensions
{
  public static class FunctionTypeExtensions
  {
    public static void SetJoyStick(this FunctionType e, JoystickOffset? joystick)
    {
      Configuration.Set(Enum.GetName(e)!, joystick is null ? null! : Enum.GetName((JoystickOffset)joystick)!);
    }
  }
}