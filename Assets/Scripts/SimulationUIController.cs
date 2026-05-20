using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Main UI controller that binds the UI Toolkit interface to the simulation.
/// Manages:
///   - PID parameter sliders (Kp, Ki, Kd)
///   - Vehicle speed control
///   - Controller mode selection (P / PI / PID)
///   - Camera mode toggle
///   - Real-time diagnostic displays
///   - Graph texture display
/// 
/// Uses UI Toolkit (UXML/USS) for a modern, glassmorphism-themed interface.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class SimulationUIController : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // References
    // ─────────────────────────────────────────────
    [Header("References")]
    public VehicleController vehicleController;
    public CameraController cameraController;
    public GraphRenderer graphRenderer;
    public CSVExporter csvExporter;

    // ─────────────────────────────────────────────
    // UI Elements (cached)
    // ─────────────────────────────────────────────
    private Slider _kpSlider;
    private Slider _kiSlider;
    private Slider _kdSlider;
    private Slider _speedSlider;

    private Label _kpValueLabel;
    private Label _kiValueLabel;
    private Label _kdValueLabel;
    private Label _speedValueLabel;

    private Label _errorLabel;
    private Label _controlLabel;
    private Label _steeringLabel;
    private Label _modeLabel;

    private DropdownField _modeDropdown;
    private Button _cameraButton;
    private Button _resetButton;
    private Button _exportButton;
    private Button _toggleGraphsButton;

    // Graph display elements
    private VisualElement _controlGraphImage;
    private VisualElement _errorGraphImage;
    private VisualElement _pGraphImage;
    private VisualElement _iGraphImage;
    private VisualElement _dGraphImage;

    // Tab elements
    private Button _tabGeneral;
    private Button _tabPid;
    private VisualElement _contentGeneral;
    private VisualElement _contentPid;

    private UIDocument _uiDocument;

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null || _uiDocument.rootVisualElement == null) return;

        var root = _uiDocument.rootVisualElement;

        // ── Bind PID Sliders ──────────────────────
        _kpSlider = root.Q<Slider>("kp-slider");
        _kiSlider = root.Q<Slider>("ki-slider");
        _kdSlider = root.Q<Slider>("kd-slider");
        _speedSlider = root.Q<Slider>("speed-slider");

        _kpValueLabel = root.Q<Label>("kp-value");
        _kiValueLabel = root.Q<Label>("ki-value");
        _kdValueLabel = root.Q<Label>("kd-value");
        _speedValueLabel = root.Q<Label>("speed-value");

        // ── Bind Diagnostic Labels ────────────────
        _errorLabel = root.Q<Label>("error-display");
        _controlLabel = root.Q<Label>("control-display");
        _steeringLabel = root.Q<Label>("steering-display");
        _modeLabel = root.Q<Label>("mode-display");

        // ── Bind Controls ─────────────────────────
        _modeDropdown = root.Q<DropdownField>("mode-dropdown");
        _cameraButton = root.Q<Button>("camera-button");
        _resetButton = root.Q<Button>("reset-button");
        _exportButton = root.Q<Button>("export-button");
        _toggleGraphsButton = root.Q<Button>("toggle-graphs-button");

        // ── Bind Graph Areas ──────────────────────
        _controlGraphImage = root.Q<VisualElement>("control-graph");
        _errorGraphImage = root.Q<VisualElement>("error-graph");
        _pGraphImage = root.Q<VisualElement>("p-graph");
        _iGraphImage = root.Q<VisualElement>("i-graph");
        _dGraphImage = root.Q<VisualElement>("d-graph");

        // ── Bind Tabs ─────────────────────────────
        _tabGeneral = root.Q<Button>("tab-general");
        _tabPid = root.Q<Button>("tab-pid");
        _contentGeneral = root.Q<VisualElement>("content-general");
        _contentPid = root.Q<VisualElement>("content-pid");

        if (_tabGeneral != null && _tabPid != null)
        {
            _tabGeneral.clicked += () => SwitchTab(true);
            _tabPid.clicked += () => SwitchTab(false);
        }

        // ── Initialize Values ─────────────────────
        if (vehicleController != null)
        {
            if (_kpSlider != null)
            {
                _kpSlider.value = vehicleController.pidController.Kp;
                _kpSlider.RegisterValueChangedCallback(evt =>
                {
                    vehicleController.pidController.Kp = evt.newValue;
                    if (_kpValueLabel != null) _kpValueLabel.text = evt.newValue.ToString("F2");
                });
            }

            if (_kiSlider != null)
            {
                _kiSlider.value = vehicleController.pidController.Ki;
                _kiSlider.RegisterValueChangedCallback(evt =>
                {
                    vehicleController.pidController.Ki = evt.newValue;
                    if (_kiValueLabel != null) _kiValueLabel.text = evt.newValue.ToString("F2");
                });
            }

            if (_kdSlider != null)
            {
                _kdSlider.value = vehicleController.pidController.Kd;
                _kdSlider.RegisterValueChangedCallback(evt =>
                {
                    vehicleController.pidController.Kd = evt.newValue;
                    if (_kdValueLabel != null) _kdValueLabel.text = evt.newValue.ToString("F2");
                });
            }

            if (_speedSlider != null)
            {
                _speedSlider.value = vehicleController.vehicleSpeed;
                _speedSlider.RegisterValueChangedCallback(evt =>
                {
                    vehicleController.vehicleSpeed = evt.newValue;
                    if (_speedValueLabel != null) _speedValueLabel.text = evt.newValue.ToString("F1") + " m/s";
                });
            }
        }

        // ── Mode Dropdown ─────────────────────────
        if (_modeDropdown != null)
        {
            _modeDropdown.choices = new System.Collections.Generic.List<string>
            {
                "P Only", "PI Only", "PID"
            };
            _modeDropdown.index = 2; // Default: PID
            _modeDropdown.RegisterValueChangedCallback(evt =>
            {
                if (vehicleController == null) return;
                switch (evt.newValue)
                {
                    case "P Only":
                        vehicleController.pidController.mode = PIDController.ControllerMode.P_Only;
                        break;
                    case "PI Only":
                        vehicleController.pidController.mode = PIDController.ControllerMode.PI_Only;
                        break;
                    case "PID":
                        vehicleController.pidController.mode = PIDController.ControllerMode.PID;
                        break;
                }
                vehicleController.pidController.Reset();
            });
        }

        // ── Camera Toggle Button ──────────────────
        if (_cameraButton != null)
        {
            _cameraButton.clicked += () =>
            {
                if (cameraController != null)
                    cameraController.ToggleCameraMode();
            };
        }

        // ── Reset Button ──────────────────────────
        if (_resetButton != null)
        {
            _resetButton.clicked += () =>
            {
                if (vehicleController != null)
                    vehicleController.ResetSimulation();
            };
        }

        // ── Export Button ─────────────────────────
        if (_exportButton != null)
        {
            _exportButton.clicked += () =>
            {
                if (csvExporter != null)
                    csvExporter.ExportData();
            };
        }

        // ── Toggle Graphs Button ──────────────────
        if (_toggleGraphsButton != null)
        {
            _toggleGraphsButton.clicked += () =>
            {
                var rightPanel = root.Q<VisualElement>("right-panel");
                if (rightPanel != null)
                {
                    bool isVisible = rightPanel.style.display != DisplayStyle.None;
                    rightPanel.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
                }
            };
        }
    }

    private void SwitchTab(bool showGeneral)
    {
        if (_contentGeneral == null || _contentPid == null) return;

        if (showGeneral)
        {
            _contentGeneral.style.display = DisplayStyle.Flex;
            _contentPid.style.display = DisplayStyle.None;
            _tabGeneral.AddToClassList("tab-active");
            _tabPid.RemoveFromClassList("tab-active");
        }
        else
        {
            _contentGeneral.style.display = DisplayStyle.None;
            _contentPid.style.display = DisplayStyle.Flex;
            _tabGeneral.RemoveFromClassList("tab-active");
            _tabPid.AddToClassList("tab-active");
        }
    }

    private void Update()
    {
        if (vehicleController == null) return;

        // ── Update Diagnostic Labels ──────────────
        if (_errorLabel != null)
            _errorLabel.text = $"e(t): {vehicleController.CurrentLateralError:F3} m";

        if (_controlLabel != null)
            _controlLabel.text = $"u(t): {vehicleController.CurrentControlOutput:F3}";

        if (_steeringLabel != null)
            _steeringLabel.text = $"δ: {vehicleController.CurrentSteeringAngle:F1}°";

        if (_modeLabel != null)
            _modeLabel.text = $"Mode: {vehicleController.pidController.mode}";

        // ── Update Graph Textures ─────────────────
        if (graphRenderer != null)
        {
            if (_controlGraphImage != null && graphRenderer.ControlOutputTexture != null)
            {
                _controlGraphImage.style.backgroundImage = new StyleBackground(graphRenderer.ControlOutputTexture);
            }

            if (_errorGraphImage != null && graphRenderer.LateralErrorTexture != null)
            {
                _errorGraphImage.style.backgroundImage = new StyleBackground(graphRenderer.LateralErrorTexture);
            }

            if (_pGraphImage != null && graphRenderer.PGraphTexture != null)
            {
                _pGraphImage.style.backgroundImage = new StyleBackground(graphRenderer.PGraphTexture);
            }

            if (_iGraphImage != null && graphRenderer.IGraphTexture != null)
            {
                _iGraphImage.style.backgroundImage = new StyleBackground(graphRenderer.IGraphTexture);
            }

            if (_dGraphImage != null && graphRenderer.DGraphTexture != null)
            {
                _dGraphImage.style.backgroundImage = new StyleBackground(graphRenderer.DGraphTexture);
            }
        }
    }
}
