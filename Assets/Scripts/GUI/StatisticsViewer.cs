#region Copyright © ThotLab Games 2011. Licensed under the terms of the Microsoft Reciprocal Licence (Ms-RL).

// Microsoft Reciprocal License (Ms-RL)
//
// This license governs use of the accompanying software. If you use the software, you accept this
// license. If you do not accept the license, do not use the software.
//
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same
// meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
//
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and
// limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free
// copyright license to reproduce its contribution, prepare derivative works of its contribution,
// and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and
// limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free
// license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or
// otherwise dispose of its contribution in the software or derivative works of the contribution in
// the software.
//
// 3. Conditions and Limitations
// (A) Reciprocal Grants- For any file you distribute that contains code from the software (in
// source code or binary format), you must provide recipients the source code to that file along
// with a copy of this license, which license will govern that file. You may license other files
// that are entirely your own work and do not contain code from the software under any terms you
// choose.
// (B) No Trademark License- This license does not grant you rights to use any contributors' name,
// logo, or trademarks.
// (C) If you bring a patent claim against any contributor over patents that you claim are
// infringed by the software, your patent license from such contributor to the software ends
// automatically.
// (D) If you distribute any portion of the software, you must retain all copyright, patent,
// trademark, and attribution notices that are present in the software.
// (E) If you distribute any portion of the software in source code form, you may do so only under
// this license by including a complete copy of this license with your distribution. If you
// distribute any portion of the software in compiled or object code form, you may only do so under
// a license that complies with this license.
// (F) The software is licensed "as-is." You bear the risk of using it. The contributors give no
// express warranties, guarantees or conditions. You may have additional consumer rights under your
// local laws which this license cannot change. To the extent permitted under your local laws, the
// contributors exclude the implied warranties of merchantability, fitness for a particular purpose
// and non-infringement.

#endregion Copyright © ThotLab Games 2011. Licensed under the terms of the Microsoft Reciprocal Licence (Ms-RL).

using UnityEngine;

// Add to the component menu.
[AddComponentMenu("Scripts/GUI/Statistics Viewer")]

public class StatisticsViewer : WindowManager
{
    public Vector2 positionOffset = new Vector2(0, 50);
    public int minimumWindowWidth = 150;
    public int minimumColumnWidth = 120;

    public bool showFps = true;

    public float updateInterval = 0.5f;

    private float x;
    private float y;
    private int width;
    private int height;
    private Rect windowRectangle;

    private string windowTitle = "Statistics Viewer";

    private int previousRows = 0;
    private int previousColumns = 0;

    private float fps;
    private float accumulatedFps = 0.0f; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeLeft; // Left time for current interval

    // If this behaviour is enabled, Start is called once
    // after all Awake calls and before all any Update calls.
    public new void Start()
    {
        base.Start(); // initializes the window id
        timeLeft = updateInterval;
    }

    // If this behaviour is enabled, Update is called once per frame.
    public void Update()
    {
        timeLeft -= Time.deltaTime;
        accumulatedFps += Time.timeScale / Time.deltaTime;
        ++frames;

        // Interval ended - update fps and start new interval
        if (timeLeft <= 0.0f)
        {
            fps = accumulatedFps / frames;
            timeLeft = updateInterval;
            accumulatedFps = 0.0f;
            frames = 0;
        }
    }

    // If this behaviour is enabled, OnGUI is called for rendering and handling GUI events.
    // It might be called several times per frame (one call per event).
    public void OnGUI()
    {
        if (width != Screen.width || height != Screen.height)
        {
            x = Screen.width * 0.02f + positionOffset.x;
            y = Screen.height * 0.02f + positionOffset.y;
            width = Screen.width;
            height = Screen.height;
            windowRectangle = new Rect(x, y, minimumWindowWidth, 0); // GUILayout will determine height
        }

        windowRectangle =
            GUILayout.Window(
                windowId,
                windowRectangle,
                WindowFunction,
                windowTitle,
                GUILayout.MinWidth(minimumWindowWidth));
    }

    // This creates the GUI inside the window.
    // It requires the id of the window it's currently making GUI for.
    private void WindowFunction(int windowID)
    {
        // Draw any Controls inside the window here.

        int maxRows = 0;
        int rows = 0;
        int columns = 0;

        // Layout dummy label to keep the title showing
        // even when no items are selected.
        GUILayout.Label("", GUIStyle.none);

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.MinWidth(minimumColumnWidth));

        rows = 0;

        if (showFps)
        {
            GUILayout.Label("FPS: " + fps.ToString("f1"));
            rows++;
        }

        maxRows = Mathf.Max(maxRows, rows);

        columns += (rows == 0) ? 0 : 1;

        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.MinWidth(minimumColumnWidth));

        rows = 0;

        //if (showAnotherColumn)
        //{
        //    GUILayout.Label("Another Column");
        //    rows++;
        //}

        maxRows = Mathf.Max(maxRows, rows);

        columns += (rows == 0) ? 0 : 1;

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        // Make the windows be draggable.
        GUI.DragWindow();

        // if the numer of rows changes we should resize
        if (previousRows != maxRows || previousColumns != columns)
        {
            windowRectangle.height = 0; // GUILayout will determine height
            windowRectangle.width = minimumWindowWidth;
            previousRows = maxRows;
            previousColumns = columns;
        }
    }
}