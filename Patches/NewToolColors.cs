using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GlobalSettings;
using HarmonyLib;
using Needleforge.Data;
using TeamCherry.NestedFadeGroup;
using UnityEngine;
using static InventoryFloatingToolSlots;

namespace Needleforge.Patches;

[HarmonyPatch(typeof(InventoryItemToolManager), nameof(InventoryItemToolManager.Awake))]
public class AddHeaders
{
    [HarmonyPostfix]
    public static void Postfix(InventoryItemToolManager __instance)
    {
        foreach (var color in NeedleforgePlugin.newColors)
        {
            __instance.listSectionHeaders[(int)color.type] = __instance.listSectionHeaders[1];
        }
    }
}

[HarmonyPatch(typeof(InventoryItemTool), nameof(InventoryItemTool.SetData))]
public class AddAnimators
{
    [HarmonyPrefix]
    public static void Postfix(InventoryItemTool __instance, ToolItem newItemData)
    {
        List<RuntimeAnimatorController> newControllers = [..__instance.slotAnimatorControllers];
        foreach (var color in NeedleforgePlugin.newColors)
        {
            newControllers.Add(__instance.slotAnimatorControllers[1]);
        }
        __instance.slotAnimatorControllers = newControllers.ToArray();
    }
}

[HarmonyPatch(typeof(UI), nameof(UI.GetToolTypeColor))]
public class NewToolColors
{
    [HarmonyPrefix]
    public static bool Prefix(ToolItemType type, ref Color __result)
    {
        if ((int)type > 3)
        {
            __result = NeedleforgePlugin.newColors[(int)type - 4].color;
            return false;
        }
        return true;
    }
}
[HarmonyPatch(typeof(InventoryItemToolManager))]
[HarmonyPatch("GetAvailableSlot")]
public static class GetAvailableSlotPatch
{
    public static InventoryToolCrestSlot ReturnValidSlot(ColorData toolColorData, ColorData slotColorData, InventoryToolCrestSlot slot, ToolItemType toolType)
    {
        if (toolColorData != null)
        {
            if (toolColorData.toolAcceptAll)
            {
                return slot;
            }
            if (toolColorData.toolAcceptedList != null)
            {
                if (toolColorData.toolAcceptedList.Contains(slot.Type))
                {
                    return slot;

                }
            }
            else if (toolColorData.type == slot.Type)
            {
                return slot;
            }
        }

        if (slotColorData != null)
        {
            if (slotColorData.toolAcceptAll)
            {
                return slot;
            }
            if (slotColorData.slotAcceptedList != null)
            {
                if (slotColorData.slotAcceptedList.Contains(toolType))
                {
                    return slot;
                }
            }
            else if (slotColorData.type == toolType)
            {
                return slot;
            }
        }
        if ((int)slot.Type <= 3)
        {
            if (toolType == slot.Type)
            {
                return slot;
            }
        }
        return null;
    }
    public static bool Prefix(InventoryItemToolManager __instance, IEnumerable<InventoryToolCrestSlot> slots, ToolItemType toolType, ref InventoryToolCrestSlot __result)
    {
        if ((int)toolType <= 3 && !(__instance.crestList?.CurrentCrest?.CrestData?.slots?.Any(slot => (int)slot.Type > 3) ?? false))
        {
            return true;
        }

        InventoryToolCrestSlot firstCandidate = null;

        foreach (InventoryToolCrestSlot slot in slots)
        {
            if (slot.IsLocked) continue;
            ColorData toolColorData = null;
            if ((int)toolType > 3)
            {
                toolColorData = NeedleforgePlugin.newColors[(int)toolType - 4];
            }
            ColorData slotColorData = null;
            if ((int)slot.Type > 3)
            {
                slotColorData = NeedleforgePlugin.newColors[(int)slot.Type - 4];
            }
            var maybeCandidate = ReturnValidSlot(toolColorData, slotColorData, slot, toolType);
            if (firstCandidate == null && maybeCandidate != null)
            {
                firstCandidate = maybeCandidate;
            }

            if (slot.EquippedItem)
            {
                continue;
            }
            var maybeSlot = ReturnValidSlot(toolColorData, slotColorData, slot, toolType);
            if (maybeSlot != null)
            {
                __result = maybeSlot;
                return false;
            }
        }

        __result = firstCandidate;
        return false;
    }
}
[HarmonyPatch(typeof(ToolItemTypeExtensions), "IsAttackType")]
public static class ToolItemType_IsAttackType_Patch
{
    static bool Prefix(ToolItemType type, ref bool __result)
    {
        if ((int)type <= 3) return true;
        __result = NeedleforgePlugin.newColors[(int)type - 4].isAttackType;
        return false;
    }
}
//[HarmonyPatch(typeof(InventoryItemToolManager), "TryPickupOrPlaceTool")]
//public static class InventoryItemToolManager_TryPickupOrPlaceTool_Patch
//{
//    static bool Prefix(InventoryItemToolManager __instance, ToolItem tool, ref bool __result)
//    {
//        __instance.PickedUpTool = tool;

//        if (tool == null)
//        {
//            __result = false;
//            return false;
//        }

//        IEnumerable<InventoryToolCrestSlot> chosenSlots = null;
//        IEnumerable<InventoryToolCrestSlot> crestSlots = null;
//        IEnumerable<InventoryToolCrestSlot> extraSlots = null;

//        if (__instance.crestList != null)
//        {
//            crestSlots = __instance.crestList.GetSlots();
//            if (InventoryItemToolManager.GetAvailableSlotCount(crestSlots, tool.Type, true) > 0)
//            {
//                chosenSlots = crestSlots;
//            }
//        }

//        if (chosenSlots == null && __instance.extraSlots != null)
//        {
//            extraSlots = __instance.extraSlots.GetSlots();
//            if (InventoryItemToolManager.GetAvailableSlotCount(extraSlots, tool.Type, true) > 0)
//            {
//                chosenSlots = extraSlots;
//            }
//        }

//        if (chosenSlots == null)
//        {
//            if (InventoryItemToolManager.GetAvailableSlotCount(crestSlots, tool.Type, false) > 0)
//            {
//                chosenSlots = crestSlots;
//            }
//            else if (InventoryItemToolManager.GetAvailableSlotCount(extraSlots, tool.Type, false) > 0)
//            {
//                chosenSlots = extraSlots;
//            }
//        }

//        if (chosenSlots != null)
//        {
//            InventoryToolCrestSlot availableSlot = __instance.GetAvailableSlot(chosenSlots, tool.Type);
//            if (availableSlot != null)
//            {
//                __instance.EquipState = InventoryItemToolManager.EquipStates.PlaceTool;
//                __instance.selectedBeforePickup = __instance.CurrentSelected;

//                if (availableSlot.Type.IsAttackType())
//                {
//                    if (InventoryItemToolManager.GetAvailableSlotCount(chosenSlots, availableSlot.Type, false) == 1)
//                    {
//                        __instance.PlaceTool(availableSlot, true);
//                    }
//                    else
//                    {
//                        __instance.PlayMoveSound();
//                        __instance.SetSelected(availableSlot, null, false);
//                    }
//                }
//                else if (InventoryItemToolManager.GetAvailableSlotCount(chosenSlots, availableSlot.Type, true) > 0)
//                {

//                    __instance.PlaceTool(availableSlot, true);
//                }
//                else
//                {
//                    int countCrest = InventoryItemToolManager.GetAvailableSlotCount(crestSlots, availableSlot.Type, false);
//                    int countExtra = InventoryItemToolManager.GetAvailableSlotCount(extraSlots, availableSlot.Type, false);
//                    if (countCrest + countExtra == 1)
//                    {
//                        __instance.PlaceTool(availableSlot, true);
//                    }
//                    else
//                    {
//                        __instance.PlayMoveSound();
//                        __instance.SetSelected(availableSlot, null, false);
//                    }
//                }
//                __instance.RefreshTools();
//                __result = true;
//                return false;
//            }
//        }
//        __instance.PickedUpTool = null;
//        __result = false;
//        return false;
//    }
//}
[HarmonyPatch(typeof(InventoryItemToolManager), "GetAvailableSlotCount")]
public static class InventoryItemToolManager_GetAvailableSlotCount_Patch
{
    static bool Prefix(IEnumerable<InventoryToolCrestSlot> slots, ToolItemType? toolType, bool checkEmpty, ref int __result)
    {
        int count = 0;
        if (slots != null)
        {
            foreach (var slot in slots)
            {
                if (slot == null || slot.IsLocked)
                    continue;
                if (toolType.HasValue) {
                    ColorData toolColorData = null;
                    if ((int)toolType.Value > 3)
                    {
                        toolColorData = NeedleforgePlugin.newColors[(int)toolType - 4];
                    }
                    ColorData slotColorData = null;
                    if ((int)slot.Type > 3)
                    {
                        slotColorData = NeedleforgePlugin.newColors[(int)slot.Type - 4];
                    }
                    bool match =
                        (toolColorData != null && (
                            toolColorData.toolAcceptAll ||
                            (toolColorData.toolAcceptedList != null && toolColorData.toolAcceptedList.Contains(slot.Type)) ||
                            (toolColorData.toolAcceptedList == null && toolColorData.type == slot.Type)
                        ))
                        ||
                        (slotColorData != null && (
                            slotColorData.toolAcceptAll ||
                            (slotColorData.slotAcceptedList != null && slotColorData.slotAcceptedList.Contains(toolType.Value)) ||
                            (slotColorData.slotAcceptedList == null && slotColorData.type == toolType.Value)
                        ))
                        ||
                        ((int)slot.Type <= 3 && toolType == slot.Type);
                    if (!match)
                        continue;
                }
                if (checkEmpty && slot.EquippedItem != null)
                    continue;
                count++;
            }
        }
        __result = count;
        return false;
    }
}
[HarmonyPatch(typeof(InventoryItemToolManager), "PlaceTool")]
public static class InventoryItemToolManager_PlaceTool_Patch
{
    static bool Prefix(InventoryItemToolManager __instance, InventoryToolCrestSlot slot, bool isManual)
    {
        void Selected()
        {
            __instance.SetSelected(__instance.selectedBeforePickup, null, false);
            __instance.selectedBeforePickup = null;
        }
        ColorData toolColorData = null;
        if ((int)__instance.PickedUpTool.Type > 3)
        {
            toolColorData = NeedleforgePlugin.newColors[(int)__instance.PickedUpTool.Type - 4];
        }
        ColorData slotColorData = null;
        if ((int)slot.Type > 3)
        {
            slotColorData = NeedleforgePlugin.newColors[(int)slot.Type - 4];
        }
        bool match =
            (toolColorData != null && (
                toolColorData.toolAcceptAll ||
                (toolColorData.toolAcceptedList != null && toolColorData.toolAcceptedList.Contains(slot.Type)) ||
                (toolColorData.toolAcceptedList == null && toolColorData.type == slot.Type)
            ))
            ||
            (slotColorData != null && (
                slotColorData.toolAcceptAll ||
                (slotColorData.slotAcceptedList != null && slotColorData.slotAcceptedList.Contains(__instance.PickedUpTool.Type)) ||
                (slotColorData.slotAcceptedList == null && slotColorData.type == __instance.PickedUpTool.Type)
            ))
            ||
            ((int)slot.Type <= 3 && __instance.PickedUpTool.Type == slot.Type);
        
        if (slot != null && !match)
            return false;

        ToolItem pickedUp = __instance.PickedUpTool;
        __instance.PickedUpTool = null;
        __instance.EquipState = InventoryItemToolManager.EquipStates.None;

        if (isManual)
            slot.SetEquipped(pickedUp, true, true);

        if (__instance.selectedBeforePickup == null)
            return false;

        if (isManual)
            slot.PreOpenSlot();

        if (__instance.tweenTool != null && slot != null)
        {
            __instance.tweenTool.DoPlace(
                __instance.selectedBeforePickup.transform.position,
                slot.transform.position,
                pickedUp,
                (Action)(() => Selected())
            );
            return false;
        }

        Selected();
        return false;
    }
}
[HarmonyPatch(typeof(EnumExtenstions), nameof(EnumExtenstions.GetValuesWithOrder), typeof(Type))]
public class ToolItemTypeEnumPatch
{
    [HarmonyPostfix]
    public static void Postfix(Type type, ref IEnumerable<int> __result)
    {
        if (type == typeof(ToolItemType))
        {
            for (int i = 0; i < NeedleforgePlugin.newColors.Count; i++)
            {
                int index = i + 4;
                __result.AddItem(index);
            }
        }
    }
}

[HarmonyPatch(typeof(Enum), nameof(Enum.GetValues), typeof(Type))]
public class ToolItemTypePatch2
{
    [HarmonyPostfix]
    public static void Postfix(Type enumType, ref Array __result)
    {
        if (enumType == typeof(ToolItemType))
        {
            List<ToolItemType> arrList = [];
            foreach (var color in __result)
            {
                arrList.Add((ToolItemType)color);
            }
            foreach (var color in NeedleforgePlugin.newColors)
            {
                arrList.Add(color.type);
            }
            __result = arrList.ToArray();
        }
    }
}

[HarmonyPatch(typeof(InventoryToolCrest), nameof(InventoryToolCrest.OnValidate))]
public class InventoryToolCrestPatches
{
    [HarmonyPostfix]
    public static void Postfix(InventoryToolCrest __instance)
    {
        foreach (var color in NeedleforgePlugin.newColors) 
        {
            __instance.templateSlots[(int)color.type] = __instance.templateSlots[1];
        }
    }
}