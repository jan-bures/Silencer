using KSP.UI.Binding;
using Silencer.Data;
using SpaceWarp.API.Assets;
using SpaceWarp.API.UI.Appbar;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Silencer.UI;

/// <summary>
/// Controller for the UI window.
/// </summary>
public class UIController : MonoBehaviour
{
    // AppBar button IDs
    private const string ToolbarFlightButtonID = "BTN-SilencerFlight";
    private const string ToolbarOabButtonID = "BTN-SilencerOAB";
    private const string ToolbarKscButtonID = "BTN-SilencerKSC";

    private readonly StorageManager _storage = SilencerPlugin.Instance.Storage;
    private VisualTreeAsset _listItemTemplate;
    private UIDocument _window;

    private VisualElement _rootElement;
    private TextField _stringTextField;
    private Button _addStringButton;
    private Button _addRegexButton;
    private Button _editButton;
    private Button _cancelButton;
    private ListView _dataListView;

    private bool _isWindowOpen;
    private SavedItem _selectedItem;

    /// <summary>
    /// The state of the window. Setting this value will open or close the window.
    /// </summary>
    public bool IsWindowOpen
    {
        get => _isWindowOpen;
        set
        {
            _isWindowOpen = value;

            // Update the Flight AppBar button state
            GameObject.Find(ToolbarFlightButtonID)
                ?.GetComponent<UIValue_WriteBool_Toggle>()
                ?.SetValue(_isWindowOpen);

            // Update the OAB AppBar button state
            GameObject.Find(ToolbarOabButtonID)
                ?.GetComponent<UIValue_WriteBool_Toggle>()
                ?.SetValue(_isWindowOpen);

            _rootElement.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void Awake()
    {
        var icon = AssetManager.GetAsset<Texture2D>($"{SilencerPlugin.ModGuid}/silencer_ui/images/shh.png");

        // Register Flight AppBar button
        Appbar.RegisterAppButton(
            SilencerPlugin.ModName,
            ToolbarFlightButtonID,
            icon,
            isOpen => IsWindowOpen = isOpen
        );

        // Register OAB AppBar Button
        Appbar.RegisterOABAppButton(
            SilencerPlugin.ModName,
            ToolbarOabButtonID,
            icon,
            isOpen => IsWindowOpen = isOpen
        );

        // Register KSC AppBar Button
        Appbar.RegisterKSCAppButton(
            SilencerPlugin.ModName,
            ToolbarKscButtonID,
            icon,
            () => IsWindowOpen = !IsWindowOpen
        );

        // Load the UI from the asset bundle
        _listItemTemplate = AssetManager.GetAsset<VisualTreeAsset>(
            $"{SilencerPlugin.ModGuid}/silencer_ui/ui/silencerlistitem.uxml"
        );

        _window = GetComponent<UIDocument>();
        _rootElement = _window.rootVisualElement[0];

        IsWindowOpen = false;
    }

    /// <summary>
    /// Runs when the window is first created, and every time the window is re-enabled.
    /// </summary>
    private void OnEnable()
    {
        _rootElement = _window.rootVisualElement[0];

        _stringTextField = _rootElement.Q<TextField>("string-input");
        _addStringButton = _rootElement.Q<Button>("add-string-button");
        _addRegexButton = _rootElement.Q<Button>("add-regex-button");
        _editButton = _rootElement.Q<Button>("edit-button");
        _cancelButton = _rootElement.Q<Button>("cancel-button");
        _dataListView = _rootElement.Q<ListView>("saved-strings-container");

        _rootElement.CenterByDefault();

        // Set up list view
        _dataListView.makeItem = () => _listItemTemplate.Instantiate();
        _dataListView.bindItem = BindListItem;
        _dataListView.itemsSource = _storage.Data;
        _dataListView.itemsChosen += _ => { };
        _dataListView.selectedIndicesChanged += indices =>
        {
            var items = indices.Select(index => _storage.Data[index]).ToList();
            _selectedItem = items.FirstOrDefault();
            _stringTextField.value = _selectedItem?.Value ?? "";
            SetVisibility(_addStringButton, _selectedItem == null);
            SetVisibility(_addRegexButton, _selectedItem == null);
            SetVisibility(_editButton, _selectedItem != null);
            SetVisibility(_cancelButton, _selectedItem != null);
        };
        _dataListView.StopMouseEventsPropagation();

        _dataListView.Rebuild();

        // Handle the close button
        var closeButton = _rootElement.Q<Button>("close-button");
        closeButton.clicked += () => IsWindowOpen = false;

        // Handle the main buttons
        _addStringButton.clicked += AddString;
        _addRegexButton.clicked += AddRegex;
        _editButton.clicked += EditString;
        _cancelButton.clicked += ClearSelection;
    }

    private void BindListItem(VisualElement item, int index)
    {
        var data = _storage.Data[index];
        item.userData = data;
        item.Q<Label>().text = data.ToString();
        item.Q<Button>().clicked += () => DeleteString(data);
    }

    private void AddString()
    {
        string newString = _stringTextField.value;
        _storage.Add(newString, SavedItemType.PlainString);
        UpdateListView();
    }

    private void AddRegex()
    {
        string newString = _stringTextField.value;
        _storage.Add(newString, SavedItemType.Regex);
        UpdateListView();
    }

    private void EditString()
    {
        string newString = _stringTextField.value;
        _storage.Edit(_selectedItem, newString);
        UpdateListView(_selectedItem);
    }

    private void DeleteString(SavedItem data)
    {
        bool isSelected = data == _selectedItem;
        _storage.Remove(data);
        UpdateListView(clearSelection: isSelected);
    }

    private void UpdateListView(SavedItem item = null, bool clearSelection = true)
    {
        if (item != null)
        {
            _dataListView.RefreshItem(_storage.Data.IndexOf(item));
        }
        else
        {
            _dataListView.Rebuild();
        }

        if (clearSelection)
        {
            ClearSelection();
        }
    }

    private void ClearSelection()
    {
        _dataListView.ClearSelection();
        _selectedItem = null;
        _stringTextField.value = "";
    }

    private static void SetVisibility(VisualElement element, bool visible)
    {
        element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}