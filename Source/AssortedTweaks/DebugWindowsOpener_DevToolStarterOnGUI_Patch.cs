using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace AssortedTweaks
{
  [HarmonyPatch(typeof (DebugWindowsOpener), "DevToolStarterOnGUI")]
  public class DebugWindowsOpener_DevToolStarterOnGUI_Patch
  {
    public static Action DrawInfoCached;

    [HarmonyPrefix]
    public static bool DebugWindowsOpener_DevToolStarterOnGUI_Prefix()
    {
      if (KeyBindingDefOf.Dev_ToggleDevMode.KeyDownEvent)
      {
        Prefs.DevMode = !Prefs.DevMode;
        Event.current.Use();
      }
      if (DebugWindowsOpener_DevToolStarterOnGUI_Patch.DrawInfoCached == null)
        DebugWindowsOpener_DevToolStarterOnGUI_Patch.DrawInfoCached = (Action) (() =>
        {
          FieldInfo fi = typeof(WidgetRow).GetField("widgetRow", BindingFlags.NonPublic | BindingFlags.Instance);

            WidgetRow row = new WidgetRow();
            ((WidgetRow)fi.GetValue(row)).Init(0.0f, 0.0f);
            ((WidgetRow)fi.GetValue(row)).Label(DebugSettings.godMode ? "Dev mode. God mode active" : "Dev mode");
            //row.Init(0.0f, 0.0f);
            //row.Label(DebugSettings.godMode ? "Dev mode. God mode active" : "Dev mode");

            //fi.SetValue(new WidgetRow(), row);
        });
      return true;
    }
  }
}
