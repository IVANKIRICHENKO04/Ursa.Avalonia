﻿using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace Ursa.Controls;

[PseudoClasses(PC_Selected)]
[TemplatePart(PART_ItemsControl, typeof(ItemsControl))]
public class Rating : TemplatedControl
{
    public const string PART_ItemsControl = "PART_ItemsControl";
    protected const string PC_Selected = ":selected";

    private ItemsControl? _itemsControl;

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<Rating, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> AllowClearProperty =
        AvaloniaProperty.Register<Rating, bool>(nameof(AllowClear), true);

    public static readonly StyledProperty<bool> AllowHalfProperty =
        AvaloniaProperty.Register<Rating, bool>(nameof(AllowHalf));

    public static readonly StyledProperty<bool> AllowFocusProperty =
        AvaloniaProperty.Register<Rating, bool>(nameof(AllowFocus));

    public static readonly StyledProperty<object> CharacterProperty =
        AvaloniaProperty.Register<Rating, object>(nameof(Character));

    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<Rating, int>(nameof(Count), 5);

    public static readonly StyledProperty<double> DefaultValueProperty =
        AvaloniaProperty.Register<Rating, double>(nameof(DefaultValue));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<Rating, IDataTemplate?>(nameof(ItemTemplate));

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool AllowClear
    {
        get => GetValue(AllowClearProperty);
        set => SetValue(AllowClearProperty, value);
    }

    public bool AllowHalf
    {
        get => GetValue(AllowHalfProperty);
        set => SetValue(AllowHalfProperty, value);
    }

    public bool AllowFocus
    {
        get => GetValue(AllowFocusProperty);
        set => SetValue(AllowFocusProperty, value);
    }

    public object Character
    {
        get => GetValue(CharacterProperty);
        set => SetValue(CharacterProperty, value);
    }

    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    public double DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly DirectProperty<Rating, AvaloniaList<RatingCharacter>> ItemsProperty =
        AvaloniaProperty.RegisterDirect<Rating, AvaloniaList<RatingCharacter>>(
            nameof(Items), o => o.Items);

    private AvaloniaList<RatingCharacter> _items;

    public AvaloniaList<RatingCharacter> Items
    {
        get => _items;
        private set => SetAndRaise(ItemsProperty, ref _items, value);
    }

    public Rating()
    {
        Items = [];
    }

    static Rating()
    {
        ValueProperty.Changed.AddClassHandler<Rating>((s, e) => s.OnValueChanged(e));
        CountProperty.Changed.AddClassHandler<Rating>((s, e) => s.OnCountChanged(e));
        AllowHalfProperty.Changed.AddClassHandler<Rating>((s, e) => s.OnAllowHalfChanged(e));
    }

    private void OnValueChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is double newValue)
        {
            UpdateItemsByValue(newValue);
        }
    }

    private void OnCountChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (!IsLoaded) return;

        var currentCount = Items.Count;
        var newCount = e.GetNewValue<int>();

        if (currentCount < newCount)
        {
            var itemsToAdd = newCount - currentCount;
            for (var i = 0; i < itemsToAdd; i++)
            {
                Items.Add(new RatingCharacter());
            }
        }
        else if (currentCount > newCount)
        {
            var itemsToRemove = currentCount - newCount;
            for (var i = 0; i < itemsToRemove && currentCount > i; i++)
            {
                Items.RemoveAt(currentCount - i - 1);
            }
        }

        foreach (var item in Items)
        {
            item.AllowHalf = AllowHalf;
        }

        UpdateItemsByValue(Value);
    }

    private void OnAllowHalfChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not bool newValue) return;
        foreach (var item in Items)
        {
            item.AllowHalf = newValue;
        }

        UpdateItemsByValue(Value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _itemsControl = e.NameScope.Find<ItemsControl>(PART_ItemsControl);
        for (var i = 0; i < Count; i++)
        {
            Items.Add(new RatingCharacter());
        }

        foreach (var item in Items)
        {
            item.AllowHalf = AllowHalf;
        }

        SetCurrentValue(ValueProperty, DefaultValue);
    }

    internal void PointerEnteredHandler(RatingCharacter o)
    {
        var index = Items.IndexOf(o);
        var item = Items.FirstOrDefault(item => item.IsLast);
        if (item is not null)
        {
            item.IsHalf = false;
        }

        UpdateItemsByIndex(index);
    }

    internal void PointerReleasedHandler(RatingCharacter o)
    {
        var index = Items.IndexOf(o);
        double newValue = index + 1;
        if (AllowHalf && o.IsHalf)
        {
            newValue = index + 0.5;
        }

        if (AllowClear && Math.Abs(Value - newValue) < double.Epsilon)
        {
            SetCurrentValue(ValueProperty, 0);
        }
        else
        {
            SetCurrentValue(ValueProperty, newValue);
        }
    }

    internal void UpdateItemsByValue(double newValue)
    {
        RestorePreviousLastItem();
        var index = (int)Math.Ceiling(newValue - 1);
        UpdateItemsByIndex(index);
        UpdateChosenItem(newValue);
    }

    private void RestorePreviousLastItem()
    {
        if (!AllowHalf) return;
        var item = Items.FirstOrDefault(item => item.IsLast);
        if (item is null) return;
        item.Ratio = 1;
        item.ApplyRatio();
    }

    private void UpdateItemsByIndex(int index)
    {
        for (var i = 0; i < Items.Count; i++)
        {
            Items[i].SetSelectedState(i <= index);
            Items[i].IsLast = i == index;
        }
    }

    private void UpdateChosenItem(double newValue)
    {
        var ratio = newValue - Math.Floor(newValue);
        var isFraction = ratio >= double.Epsilon;
        ratio = AllowHalf && isFraction ? ratio : 1;
        var item = Items.FirstOrDefault(item => item.IsLast);
        if (item is null) return;
        if (!AllowHalf && isFraction)
        {
            item.SetSelectedState(false);
        }

        item.Ratio = ratio;
        item.ApplyRatio();
    }
}