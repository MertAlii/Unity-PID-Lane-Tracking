using UnityEngine;

/// <summary>
/// High-performance real-time graph renderer using Texture2D.
/// Draws control system signals (e(t), u(t), r(t), y(t)) as
/// scrolling time-series graphs directly onto textures.
/// 
/// Two graphs are rendered:
///   1. Control Output u(t) — shows the PID steering signal over time
///   2. Lateral Error e(t) — shows tracking error with reference line r(t)=0
/// 
/// Performance: Uses SetPixels32 for fast pixel manipulation,
/// only redraws when new data arrives.
/// </summary>
public class GraphRenderer : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // Graph Dimensions
    // ─────────────────────────────────────────────
    [Header("Graph Settings")]
    [Tooltip("Width of each graph texture in pixels")]
    public int graphWidth = 400;

    [Tooltip("Height of each graph texture in pixels")]
    public int graphHeight = 150;

    [Tooltip("Number of data points visible on screen")]
    public int visibleDataPoints = 300;

    // ─────────────────────────────────────────────
    // Colors (AMOLED-friendly dark theme)
    // ─────────────────────────────────────────────
    [Header("Colors")]
    public Color backgroundColor = new Color(0.06f, 0.06f, 0.12f, 0.9f);     // Near-black
    public Color gridColor = new Color(0.15f, 0.15f, 0.25f, 0.5f);            // Subtle grid
    public Color axisColor = new Color(0.3f, 0.3f, 0.5f, 0.8f);               // Axis lines
    public Color controlOutputColor = new Color(0f, 0.9f, 1f, 1f);            // Cyan for u(t)
    public Color lateralErrorColor = new Color(1f, 0.3f, 0.3f, 1f);           // Red for e(t)
    public Color referenceColor = new Color(0.2f, 1f, 0.4f, 0.6f);            // Green for r(t)=0
    public Color pTermColor = new Color(1f, 0.6f, 0f, 1f);                    // Orange for P
    public Color iTermColor = new Color(0.8f, 0.2f, 1f, 1f);                  // Purple for I
    public Color dTermColor = new Color(1f, 1f, 0f, 1f);                      // Yellow for D

    // ─────────────────────────────────────────────
    // Scale
    // ─────────────────────────────────────────────
    [Header("Scale")]
    [Tooltip("Y-axis range for control output graph")]
    public float controlOutputRange = 1.5f;

    [Tooltip("Y-axis range for lateral error graph")]
    public float lateralErrorRange = 5f;

    [Tooltip("Y-axis range for PID component graphs")]
    public float pidComponentRange = 5f;

    // ─────────────────────────────────────────────
    // References
    // ─────────────────────────────────────────────
    [Header("References")]
    public VehicleController vehicleController;

    // ─────────────────────────────────────────────
    // Output Textures (assign to UI RawImage or material)
    // ─────────────────────────────────────────────
    private Texture2D _controlOutputTexture;
    private Texture2D _lateralErrorTexture;
    private Texture2D _pGraphTexture;
    private Texture2D _iGraphTexture;
    private Texture2D _dGraphTexture;

    private Color32[] _controlPixels;
    private Color32[] _errorPixels;
    private Color32[] _pPixels;
    private Color32[] _iPixels;
    private Color32[] _dPixels;

    /// <summary>Texture showing u(t) control output over time</summary>
    public Texture2D ControlOutputTexture => _controlOutputTexture;

    /// <summary>Texture showing e(t) lateral error over time</summary>
    public Texture2D LateralErrorTexture => _lateralErrorTexture;

    public Texture2D PGraphTexture => _pGraphTexture;
    public Texture2D IGraphTexture => _iGraphTexture;
    public Texture2D DGraphTexture => _dGraphTexture;

    private void Awake()
    {
        CreateTextures();
    }

    private void Update()
    {
        if (vehicleController == null) return;

        RedrawControlOutputGraph();
        RedrawLateralErrorGraph();
        RedrawPIDGraphs();
    }

    /// <summary>
    /// Initializes the graph textures with proper settings.
    /// </summary>
    private void CreateTextures()
    {
        _controlOutputTexture = CreateGraphTexture();
        _lateralErrorTexture = CreateGraphTexture();
        _pGraphTexture = CreateGraphTexture();
        _iGraphTexture = CreateGraphTexture();
        _dGraphTexture = CreateGraphTexture();

        int pixelCount = graphWidth * graphHeight;
        _controlPixels = new Color32[pixelCount];
        _errorPixels = new Color32[pixelCount];
        _pPixels = new Color32[pixelCount];
        _iPixels = new Color32[pixelCount];
        _dPixels = new Color32[pixelCount];
    }

    private Texture2D CreateGraphTexture()
    {
        var tex = new Texture2D(graphWidth, graphHeight, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    /// <summary>
    /// Redraws the control output u(t) graph with latest data.
    /// Shows the PID controller's steering command over time.
    /// </summary>
    private void RedrawControlOutputGraph()
    {
        var data = vehicleController.RecordedData;
        if (data.Count < 2) return;

        // Clear background
        Color32 bg = backgroundColor;
        Color32 grid = gridColor;
        Color32 axis = axisColor;

        for (int i = 0; i < _controlPixels.Length; i++)
            _controlPixels[i] = bg;

        // Draw horizontal grid lines
        DrawHorizontalGrid(_controlPixels, grid, 5);

        // Draw zero axis (center horizontal line)
        int centerY = graphHeight / 2;
        DrawHorizontalLine(_controlPixels, centerY, axis);

        // Draw data curve
        int startIdx = Mathf.Max(0, data.Count - visibleDataPoints);
        int count = data.Count - startIdx;

        for (int i = 1; i < count && i < graphWidth; i++)
        {
            int dataIdx = startIdx + i;
            if (dataIdx >= data.Count) break;

            // Map data value to pixel Y coordinate
            float value = data[dataIdx].controlOutput;
            float prevValue = data[dataIdx - 1].controlOutput;

            int x = (int)((float)i / count * (graphWidth - 1));
            int prevX = (int)((float)(i - 1) / count * (graphWidth - 1));

            int y = ValueToPixelY(value, controlOutputRange);
            int prevY = ValueToPixelY(prevValue, controlOutputRange);

            // Draw line between consecutive points
            DrawLine(_controlPixels, prevX, prevY, x, y, controlOutputColor);
        }

        _controlOutputTexture.SetPixels32(_controlPixels);
        _controlOutputTexture.Apply();
    }

    /// <summary>
    /// Redraws the lateral error e(t) graph with latest data.
    /// Shows the tracking error and reference line r(t) = 0.
    /// </summary>
    private void RedrawLateralErrorGraph()
    {
        var data = vehicleController.RecordedData;
        if (data.Count < 2) return;

        Color32 bg = backgroundColor;
        Color32 grid = gridColor;
        Color32 axis = axisColor;

        for (int i = 0; i < _errorPixels.Length; i++)
            _errorPixels[i] = bg;

        // Draw grid
        DrawHorizontalGrid(_errorPixels, grid, 5);

        // Draw reference line r(t) = 0 (center)
        int centerY = graphHeight / 2;
        DrawHorizontalLine(_errorPixels, centerY, (Color32)referenceColor);

        // Draw error curve
        int startIdx = Mathf.Max(0, data.Count - visibleDataPoints);
        int count = data.Count - startIdx;

        for (int i = 1; i < count && i < graphWidth; i++)
        {
            int dataIdx = startIdx + i;
            if (dataIdx >= data.Count) break;

            float value = data[dataIdx].lateralError;
            float prevValue = data[dataIdx - 1].lateralError;

            int x = (int)((float)i / count * (graphWidth - 1));
            int prevX = (int)((float)(i - 1) / count * (graphWidth - 1));

            int y = ValueToPixelY(value, lateralErrorRange);
            int prevY = ValueToPixelY(prevValue, lateralErrorRange);

            DrawLine(_errorPixels, prevX, prevY, x, y, lateralErrorColor);
        }

        _lateralErrorTexture.SetPixels32(_errorPixels);
        _lateralErrorTexture.Apply();
    }

    /// <summary>
    /// Redraws the PID component graphs (P, I, D).
    /// </summary>
    private void RedrawPIDGraphs()
    {
        var data = vehicleController.RecordedData;
        if (data.Count < 2) return;

        Color32 bg = backgroundColor;
        Color32 grid = gridColor;
        Color32 axis = axisColor;

        for (int i = 0; i < _pPixels.Length; i++)
        {
            _pPixels[i] = bg;
            _iPixels[i] = bg;
            _dPixels[i] = bg;
        }

        DrawHorizontalGrid(_pPixels, grid, 3);
        DrawHorizontalGrid(_iPixels, grid, 3);
        DrawHorizontalGrid(_dPixels, grid, 3);

        int centerY = graphHeight / 2;
        DrawHorizontalLine(_pPixels, centerY, axis);
        DrawHorizontalLine(_iPixels, centerY, axis);
        DrawHorizontalLine(_dPixels, centerY, axis);

        int startIdx = Mathf.Max(0, data.Count - visibleDataPoints);
        int count = data.Count - startIdx;

        for (int i = 1; i < count && i < graphWidth; i++)
        {
            int dataIdx = startIdx + i;
            if (dataIdx >= data.Count) break;

            var current = data[dataIdx];
            var prev = data[dataIdx - 1];

            int x = (int)((float)i / count * (graphWidth - 1));
            int prevX = (int)((float)(i - 1) / count * (graphWidth - 1));

            // P
            int yP = ValueToPixelY(current.pTerm, pidComponentRange);
            int prevYP = ValueToPixelY(prev.pTerm, pidComponentRange);
            DrawLine(_pPixels, prevX, prevYP, x, yP, pTermColor);

            // I
            int yI = ValueToPixelY(current.iTerm, pidComponentRange);
            int prevYI = ValueToPixelY(prev.iTerm, pidComponentRange);
            DrawLine(_iPixels, prevX, prevYI, x, yI, iTermColor);

            // D
            int yD = ValueToPixelY(current.dTerm, pidComponentRange);
            int prevYD = ValueToPixelY(prev.dTerm, pidComponentRange);
            DrawLine(_dPixels, prevX, prevYD, x, yD, dTermColor);
        }

        _pGraphTexture.SetPixels32(_pPixels);
        _pGraphTexture.Apply();

        _iGraphTexture.SetPixels32(_iPixels);
        _iGraphTexture.Apply();

        _dGraphTexture.SetPixels32(_dPixels);
        _dGraphTexture.Apply();
    }

    // ─────────────────────────────────────────────
    // Drawing Helpers
    // ─────────────────────────────────────────────

    /// <summary>
    /// Maps a signal value to a pixel Y coordinate.
    /// Center of graph = 0, top = +range, bottom = -range.
    /// </summary>
    private int ValueToPixelY(float value, float range)
    {
        float normalized = (value / range) * 0.5f + 0.5f; // Map [-range, range] to [0, 1]
        normalized = Mathf.Clamp01(normalized);
        return (int)(normalized * (graphHeight - 1));
    }

    /// <summary>
    /// Draws a horizontal line across the full graph width.
    /// </summary>
    private void DrawHorizontalLine(Color32[] pixels, int y, Color32 color)
    {
        if (y < 0 || y >= graphHeight) return;
        for (int x = 0; x < graphWidth; x++)
        {
            pixels[y * graphWidth + x] = color;
        }
    }

    /// <summary>
    /// Draws evenly spaced horizontal grid lines.
    /// </summary>
    private void DrawHorizontalGrid(Color32[] pixels, Color32 color, int divisions)
    {
        for (int d = 0; d <= divisions; d++)
        {
            int y = (int)((float)d / divisions * (graphHeight - 1));
            DrawHorizontalLine(pixels, y, color);
        }
    }

    /// <summary>
    /// Draws a line between two points using Bresenham's algorithm.
    /// Provides smooth, anti-aliased-like graph curves.
    /// </summary>
    private void DrawLine(Color32[] pixels, int x0, int y0, int x1, int y1, Color32 color)
    {
        // Bresenham's line algorithm
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            // Set pixel with bounds check
            if (x0 >= 0 && x0 < graphWidth && y0 >= 0 && y0 < graphHeight)
            {
                pixels[y0 * graphWidth + x0] = color;

                // Draw thicker line (2px) for better visibility
                if (y0 + 1 < graphHeight)
                    pixels[(y0 + 1) * graphWidth + x0] = color;
            }

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx)  { err += dx; y0 += sy; }
        }
    }
}
