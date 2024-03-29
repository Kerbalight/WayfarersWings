﻿using BepInEx.Logging;
using UnityEngine.UIElements;
using WayfarersWings.Models.Session;
using WayfarersWings.Models.Wings;
using WayfarersWings.UI.Localization;
using WayfarersWings.Utility;

namespace WayfarersWings.UI.Components;

public class KerbalWingEntryRowController
{
    private static readonly ManualLogSource
        Logger = BepInEx.Logging.Logger.CreateLogSource("WayfarerWings.WingRowController");

    private readonly VisualElement _root;
    private readonly VisualElement _ribbonsSpace;
    private readonly Label _name;
    private readonly Label _description;
    private readonly Label _date;
    private readonly Button _deleteButton;
    private readonly Button _deleteConfirmButton;

    private KerbalWingEntry? _entry;

    public VisualElement Root => _root;

    public KerbalWingEntryRowController(VisualElement root)
    {
        _root = root;
        _ribbonsSpace = _root.Q<VisualElement>("ribbons-space");
        _name = _root.Q<Label>("name");
        _description = _root.Q<Label>("description");
        _date = _root.Q<Label>("date");
        _deleteButton = _root.Q<Button>("delete-button");
        _deleteConfirmButton = _root.Q<Button>("delete-confirm-button");
        var deleteConfirmPopover = _root.Q<VisualElement>("delete-confirm-popover");

        _deleteConfirmButton.text = LocalizedStrings.ConfirmRevokeButton;
        deleteConfirmPopover.style.display = DisplayStyle.None;
        _deleteButton.clicked += () =>
            deleteConfirmPopover.style.display = deleteConfirmPopover.style.display == DisplayStyle.None
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        _deleteConfirmButton.clicked += OnDeleteConfirm;
    }

    private void OnDeleteConfirm()
    {
        if (_entry == null)
        {
            Logger.LogError("Entry is null, cannot delete Wing");
            return;
        }

        WingsSessionManager.Instance.Revoke(_entry, _entry.KerbalId);
    }

    public static KerbalWingEntryRowController Create()
    {
        var template = MainUIManager.Instance.GetTemplate("KerbalWingEntryRow");
        var root = template.Instantiate();
        var controller = new KerbalWingEntryRowController(root);
        root.userData = controller;
        return controller;
    }

    public void Bind(KerbalWingEntry entry)
    {
        _entry = entry;
        Bind(entry.Wing);

        _date.text = DateTimeLogic.FormatUniverseTime(entry.universeTime);
        _deleteButton.style.display = DisplayStyle.Flex;
    }

    public void SetEllipsis(bool value)
    {
        _description.style.whiteSpace = value ? WhiteSpace.NoWrap : WhiteSpace.Normal;
        _description.style.textOverflow = value ? TextOverflow.Ellipsis : TextOverflow.Clip;
    }

    public void Bind(Wing wing)
    {
        _name.text = wing.DisplayName;
        _description.text = wing.Description;
        _date.text = "";

        _deleteButton.style.display = DisplayStyle.None;

        _ribbonsSpace.Clear();
        var ribbon = WingRibbonController.Create();
        ribbon.DisplayBig = true;
        ribbon.Bind(wing);
        _ribbonsSpace.Add(ribbon.Root);
    }
}