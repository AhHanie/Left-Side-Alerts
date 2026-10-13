using HarmonyLib;
using System.Runtime;
using Verse;

namespace Left_Side_Alerts
{
    public class Mod: Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            
            LongEventHandler.QueueLongEvent(Init, "Left Side Alerts Init", doAsynchronously: true, null);
        }

        public void Init()
        {
            GetSettings<ModSettings>();
            new Harmony("sk.leftsidealerts").PatchAll();
        }

        public override string SettingsCategory()
        {
            return "LeftSideAlerts.SettingsTitle".Translate();
        }

        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {
            ModSettingsWindow.Draw(inRect);
            base.DoSettingsWindowContents(inRect);
        }
    }
}
