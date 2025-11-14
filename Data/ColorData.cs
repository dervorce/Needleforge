using System.Collections.Generic;
using UnityEngine;

namespace Needleforge.Data;

public class ColorData
{
    public string name = "";
    public Color color;
    public ToolItemType type;
    public Sprite sprite;
    public List<ToolItemType> toolAcceptedList;
    public List<ToolItemType> slotAcceptedList;
    public bool toolAcceptAll = false;
    public bool slotAcceptAll = false;
    public bool isAttackType = false;
}