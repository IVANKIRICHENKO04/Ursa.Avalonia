﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Ursa.Controls;

public class AvatarGroup : ItemsControl
{
    public const string PART_RenderMore = "PART_RenderMore";

    private static readonly FuncTemplate<Panel?> DefaultPanel = new(() => new AvatarGroupPanel());

    public static readonly StyledProperty<int> MaxCountProperty = AvaloniaProperty.Register<AvatarGroup, int>(
        nameof(MaxCount));

    public static readonly StyledProperty<OverlapFromType> OverlapFromProperty =
        AvaloniaProperty.Register<AvatarGroup, OverlapFromType>(
            nameof(OverlapFrom), defaultValue: OverlapFromType.Start);

    public int MaxCount
    {
        get => GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }

    public OverlapFromType OverlapFrom
    {
        get => GetValue(OverlapFromProperty);
        set => SetValue(OverlapFromProperty, value);
    }

    static AvatarGroup()
    {
        ItemsPanelProperty.OverrideDefaultValue<AvatarGroup>(DefaultPanel);
    }
}

public enum OverlapFromType
{
    Start,
    End
}