using System.Reflection;
using System.Xml.Serialization;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;

namespace IntentionStateMachine;

[HarmonyPatch(typeof(NCreature))]
public static class IntentionStateMachinePatch
{
    private static readonly FieldInfo? TitleField = typeof(HoverTip).GetField("<Title>k__BackingField",
        BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo? DescriptionField = typeof(HoverTip).GetField("<Description>k__BackingField",
        BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo? IdField = typeof(HoverTip).GetField("<Id>k__BackingField",
        BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo? IconField = typeof(HoverTip).GetField("<Icon>k__BackingField",
        BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo? IsSmartField = typeof(HoverTip).GetField("<IsSmart>k__BackingField",
        BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo? IsDebuffField = typeof(HoverTip).GetField("<IsDebuff>k__BackingField",
        BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo? IsInstancedField = typeof(HoverTip).GetField("<IsInstanced>k__BackingField",
        BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo? CanonicalModelField = typeof(HoverTip).GetField("<CanonicalModel>k__BackingField",
        BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo? ShouldOverrideTextOverflowField = typeof(HoverTip).GetField("<ShouldOverrideTextOverflow>k__BackingField",
        BindingFlags.NonPublic | BindingFlags.Instance);
    
    [HarmonyPatch("OnFocus")]// method name
    [HarmonyPostfix]
    public static void OnfocusPatch(NCreature __instance)
    {
        if (__instance.Entity?.Monster == null) return;
        // if (NTargetManager.Instance.IsInSelection) return; //if is seleceting target, not display
        // if (!___IsFocused) return;
        
        ShowIntentTip(__instance);
    }
    [HarmonyPatch("OnUnfocus")]
    [HarmonyPostfix]
    public static void OnUnfocusPatch(NCreature __instance)
    {
        NHoverTipSet.Remove(__instance);
    }
    private static string? GetMonsterId(NCreature instance)
    {
        string typeName = instance.Entity?.Monster?.GetType().Name;
        return typeName;
    }
    private static void ShowIntentTip(NCreature instance)
    {
        string description = GetMonsterIntention.GetNextIntention(instance);
        HoverTip hoverTip = CreateCustomHoverTip(GetMonsterId(instance), "NextMove:\n" + description);

        var hoverTipSet = NHoverTipSet.CreateAndShow(instance, hoverTip);
        
        // hoverTipSet.CallDeferred(nameof(SetTipPosition), instance, hoverTipSet);
        SetTipPosition(instance, hoverTipSet);
    }
    private static void SetTipPosition(NCreature instance, NHoverTipSet hoverTipSet)
    {
        if (hoverTipSet == null || !GodotObject.IsInstanceValid(hoverTipSet)) return;
        Rect2 hitboxRect = new Rect2(instance.Hitbox.GlobalPosition, instance.Hitbox.Size);
        
        hoverTipSet.GlobalPosition = new Vector2(
           hitboxRect.Position.X 
         + hitboxRect.Size.X/2
         - hoverTipSet.Size.X/2
          , 
           hitboxRect.Position.Y
         - hoverTipSet.Size.Y - 125);
        // hoverTipSet.GlobalPosition = instance.GetTopOfHitbox() + Godot.Vector2.Up * 200f + Godot.Vector2.Left * 200f;
    
    }
    private static HoverTip CreateCustomHoverTip(string title, string description)
    {
        HoverTip hoverTip = default;
        TypedReference tr = __makeref(hoverTip);

        TitleField?.SetValueDirect(tr, title);
        DescriptionField?.SetValueDirect(tr, description);
        IdField?.SetValueDirect(tr, "intention.tooltip");
        IconField?.SetValueDirect(tr, null);
        IsSmartField?.SetValueDirect(tr, false);
        IsDebuffField?.SetValueDirect(tr, false);
        IsInstancedField?.SetValueDirect(tr, false);
        CanonicalModelField?.SetValueDirect(tr, null);
        ShouldOverrideTextOverflowField?.SetValueDirect(tr, false);

        return hoverTip; 
    }
}