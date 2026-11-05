using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace FreeStart
{
    public sealed class ModEntry : IMod
    {
        internal static bool Active;
        internal static ConfigEntry<int> Points, Slots;
        internal static ConfigEntry<bool> UnlimitedPoints, UnlimitedSlots, FirstRun;

        public void Init(ModManifest manifest)
        {
            ModConfig config = ModHelper.GetConfig(manifest.ID);
            Points = config.Define("starting_points", 99, "开局天赋", () => "初始天赋点数",
                tooltip: () => "输入 0～2147483647 的整数。0 表示零点；负数按零处理。原版为 3。应用后重新打开开局天赋界面生效。");
            Slots = config.Define("base_slots", 99, "开局天赋", () => "基础天赋槽位数",
                tooltip: () => "输入 0～2147483647 的整数。0 表示零槽；负数按零处理。原版为 4。天赋自身的额外槽位仍会叠加。");
            UnlimitedPoints = config.Define("unlimited_points", false, "开局天赋", () => "无限天赋点数",
                tooltip: () => "开启后忽略点数限制，保留上面填写的数值。");
            UnlimitedSlots = config.Define("unlimited_slots", false, "开局天赋", () => "无限天赋槽位",
                tooltip: () => "开启后忽略槽位限制。天赋互斥规则仍然有效。");
            FirstRun = config.Define("allow_first_run", true, "开局天赋", () => "首局也可选择天赋",
                tooltip: () => "允许第一次开局选择天赋，不修改游玩次数或存档。");
            new ModLog(manifest).Log("FreeStart 1.0.0 initialized. Configure points and slots with the gear button in Mods.");
        }

        public void OnEnable() { Active = true; }
        public void OnDisable() { Active = false; }

        internal static List<StartingPerk> Selected { get { return NewGameData.Instance.startingPerks; } }
        internal static int PointLimit { get { return Math.Max(0, Points.Value); } }
        internal static int SlotLimit { get { return Math.Max(0, Slots.Value); } }
        public static bool FilterFirstRun(bool original) { return original && !(Active && FirstRun.Value); }
    }

    // Long arithmetic keeps negative-cost perks and very large user values safe.
    internal static class Budget
    {
        internal static long UsedPoints(IList<StartingPerk> perks)
        {
            long used = 0;
            foreach (StartingPerk perk in perks) used += perk.cost;
            return used;
        }

        internal static int TotalSlots(IList<StartingPerk> perks, int baseSlots)
        {
            long total = Math.Max(0, baseSlots);
            foreach (StartingPerk perk in perks) total += perk.maxSlot;
            return (int)Math.Max(0L, Math.Min((long)int.MaxValue, total));
        }

        internal static bool CanStart(IList<StartingPerk> perks, int points, int slots, bool infinitePoints, bool infiniteSlots)
        {
            return (infinitePoints || UsedPoints(perks) <= Math.Max(0, points))
                && (infiniteSlots || perks.Count <= TotalSlots(perks, slots));
        }
    }

    [HarmonyPatch(typeof(NewGamePerksUI), "CanStart")]
    internal static class StartCheckPatch
    {
        private static bool Prefix(ref bool __result)
        {
            if (!ModEntry.Active) return true;
            __result = Budget.CanStart(ModEntry.Selected, ModEntry.PointLimit, ModEntry.SlotLimit,
                ModEntry.UnlimitedPoints.Value, ModEntry.UnlimitedSlots.Value);
            return false;
        }
    }

    [HarmonyPatch(typeof(NewGamePerksUI), "MaxPerkCount")]
    internal static class SlotCountPatch
    {
        private static bool Prefix(ref int __result)
        {
            if (!ModEntry.Active) return true;
            __result = ModEntry.UnlimitedSlots.Value ? int.MaxValue : Budget.TotalSlots(ModEntry.Selected, ModEntry.SlotLimit);
            return false;
        }
    }

    [HarmonyPatch(typeof(NewGamePerksUI), "RefreshCounters")]
    internal static class CountersPatch
    {
        private static bool Prefix(NewGamePerksFrame ___frame)
        {
            if (!ModEntry.Active || ___frame == null) return true;
            List<StartingPerk> selected = ModEntry.Selected;
            long remaining = (long)ModEntry.PointLimit - Budget.UsedPoints(selected);
            int totalSlots = Budget.TotalSlots(selected, ModEntry.SlotLimit);
            string points = ModEntry.UnlimitedPoints.Value ? "∞" : remaining.ToString(CultureInfo.InvariantCulture);
            string slots = selected.Count.ToString(CultureInfo.InvariantCulture) + "/"
                + (ModEntry.UnlimitedSlots.Value ? "∞" : totalSlots.ToString(CultureInfo.InvariantCulture));
            ___frame.pointsCounter.text = LocHelper.GetLocalizedMainMenu("new_perk_points_available",
                new object[] { Mark(points, !ModEntry.UnlimitedPoints.Value && remaining < 0) });
            ___frame.perksCounter.text = LocHelper.GetLocalizedMainMenu("new_perk_count",
                new object[] { Mark(slots, !ModEntry.UnlimitedSlots.Value && selected.Count > totalSlots) });
            return false;
        }

        private static string Mark(string value, bool over) { return "<color=" + (over ? "#FF7777" : "#E4BC68") + ">" + value + "</color>"; }
    }

    // Filter the existing first-run flag before the game's UI reads it.
    // No run-count/save edits and no changes to conflict checks or selection handlers.
    [HarmonyPatch(typeof(NewGamePerksUI), "Open")]
    internal static class FirstRunPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result = new List<CodeInstruction>();
            FieldInfo field = AccessTools.Field(typeof(NewGamePerksUI), "firstRun");
            MethodInfo filter = AccessTools.Method(typeof(ModEntry), "FilterFirstRun");
            int matches = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.StoresField(field))
                {
                    var call = new CodeInstruction(OpCodes.Call, filter);
                    call.labels.AddRange(instruction.labels);
                    instruction.labels.Clear();
                    call.blocks.AddRange(instruction.blocks);
                    instruction.blocks.Clear();
                    result.Add(call);
                    matches++;
                }
                result.Add(instruction);
            }
            if (matches != 1) throw new InvalidOperationException("FreeStart: unsupported game version (firstRun pattern changed).");
            return result;
        }
    }
}
