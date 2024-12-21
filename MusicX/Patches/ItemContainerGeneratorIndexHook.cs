using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace MusicX.Patches;

public static class ItemContainerGeneratorIndexHook
{
    public static readonly DependencyProperty ItemContainerIndexProperty = DependencyProperty.RegisterAttached(
        "ItemContainerIndex", typeof(int), typeof(ItemContainerGeneratorIndexHook), new PropertyMetadata(default(int)));

    public static void SetItemContainerIndex(DependencyObject element, int value)
    {
        // always set new value
        ClearItemContainerIndex(element);
        element.SetValue(ItemContainerIndexProperty, value);
    }

    public static int GetItemContainerIndex(DependencyObject element) =>
        (int)element.GetValue(ItemContainerIndexProperty);

    public static void ClearItemContainerIndex(DependencyObject element) =>
        element.ClearValue(ItemContainerIndexProperty);

    // required to keep hooks alive
    // ReSharper disable once CollectionNeverQueried.Local
    private static readonly List<IDisposable> Hooks = [];

    public static void Apply()
    {
        var generateNextMethod = typeof(ItemContainerGenerator)
            .GetNestedType("Generator", BindingFlags.NonPublic)!
            .GetMethod("GenerateNext")!;

        var generatorHostType = Type.GetType("MS.Internal.Controls.IGeneratorHost, PresentationFramework", true)!;

        var unlinkContainerFromItemMethod = typeof(ItemContainerGenerator).GetMethod("UnlinkContainerFromItem",
            BindingFlags.NonPublic | BindingFlags.Static,
            [typeof(DependencyObject), typeof(object), generatorHostType])!;

        var itemReplacedMethod = typeof(ItemContainerGenerator).GetMethod("OnItemReplaced",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        
        var itemMovedMethod = typeof(ItemContainerGenerator).GetMethod("OnItemMoved",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        
        var itemRemovedMethod = typeof(ItemContainerGenerator).GetMethod("OnItemRemoved",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        
        var itemAddedMethod = typeof(ItemContainerGenerator).GetMethod("OnItemAdded",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        
        Hooks.Add(new ILHook(generateNextMethod, GenerateNextManipulator));
        Hooks.Add(new ILHook(unlinkContainerFromItemMethod, UnlinkContainerFromItemManipulator));
        Hooks.Add(new ILHook(itemReplacedMethod, ItemReplacedManipulator));
        Hooks.Add(new Hook(itemMovedMethod, ItemMovedHook));
        Hooks.Add(new ILHook(itemMovedMethod, ItemMovedManipulator));
        Hooks.Add(new Hook(itemRemovedMethod, ItemRemovedHook));
        Hooks.Add(new Hook(itemAddedMethod, ItemAddedHook));
    }
    
    private static void ItemAddedHook(Action<ItemContainerGenerator, object, int> original, ItemContainerGenerator self, object item, int itemIndex)
    {
        original(self, item, itemIndex);
        UpdateContainerIndices(self, itemIndex);
    }

    private static void ItemRemovedHook(Action<ItemContainerGenerator, object, int> original,
        ItemContainerGenerator self, object item, int itemIndex)
    {
        original(self, item, itemIndex);
        UpdateContainerIndices(self, itemIndex);
    }

    private static void ItemMovedHook(Action<ItemContainerGenerator, object, int, int> original, ItemContainerGenerator self, object item, int oldIndex, int newIndex)
    {
        original(self, item, oldIndex, newIndex);

        UpdateContainerIndices(self, Math.Min(oldIndex, newIndex + 1));
    }

    private static void UpdateContainerIndices(ItemContainerGenerator self, int index)
    {
        while (true)
        {
            var container = self.ContainerFromIndex(index);
            
            if (container == null)
                break;
            
            SetItemContainerIndex(container, index++);
        }
    }

    private static void ItemMovedManipulator(ILContext il)
    {
        var cursor = new ILCursor(il);

        cursor.GotoNext(b => b.MatchStloc0() && !b.Previous.Match(OpCodes.Ldnull));

        cursor.EmitDup(); // container
        cursor.EmitLdarg3(); // newIndex
        cursor.EmitCall(SetMethod);
    }

    private static void ItemReplacedManipulator(ILContext il)
    {
        var cursor = new ILCursor(il);

        // existing container link
        cursor.GotoNext(MoveType.After, b => b.MatchCall(LinkMethod));

        cursor.EmitLdloc(5); // container
        cursor.EmitLdloc(3); // index
        cursor.EmitCall(SetMethod);
        
        // new container link
        cursor.GotoNext(MoveType.After, b => b.MatchCall(LinkMethod));
        
        cursor.EmitLdloc(6); // newContainer
        cursor.EmitLdloc(3); // index
        cursor.EmitCall(SetMethod);
    }

    private static void UnlinkContainerFromItemManipulator(ILContext il)
    {
        var cursor = new ILCursor(il);

        var clearMethod = typeof(ItemContainerGeneratorIndexHook).GetMethod(nameof(ClearItemContainerIndex))!;

        cursor.EmitLdarg0();
        cursor.EmitCall(clearMethod);
    }

    private static void GenerateNextManipulator(ILContext il)
    {
        var cursor = new ILCursor(il);

        cursor.GotoNext(MoveType.After, b => b.MatchCall(LinkMethod));

        cursor.EmitLdloc(0); // container
        cursor.EmitLdloc(3); // itemIndex
        cursor.EmitCall(SetMethod);
    }

    private static readonly MethodInfo LinkMethod = typeof(ItemContainerGenerator).GetMethod("LinkContainerToItem",
        BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo SetMethod =
        typeof(ItemContainerGeneratorIndexHook).GetMethod(nameof(SetItemContainerIndex))!;
    
    private static readonly MethodInfo GetMethod =
        typeof(ItemContainerGeneratorIndexHook).GetMethod(nameof(GetItemContainerIndex))!;
}