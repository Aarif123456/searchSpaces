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
[AddComponentMenu("Scripts/GUI/FPS Viewer")]

// This calculates frames/second over each updateInterval,
// so the display does not keep changing wildly.
//
// It is also fairly accurate at very low FPS counts (<10).
// We do this not by simply counting frames per interval, but
// by accumulating FPS for each frame. This way we end up with
// correct overall FPS even if the interval renders something
// like 5.5 frames.
public class FpsViewer : WindowManager
{
    public Vector2 positionOffset = new Vector2(0, 0);

    public bool alignRight = true;
    public bool alignBottom = false;

    public float updateInterval = 0.5f;

    private int width;
    private int height;
    private Rect windowRectangle;

    private string windowTitle = string.Empty;

    private float fps;
    private float accumulatedFps = 0.0f; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeLeft; // Left time for current interval

    // If this behaviour is enabled, Start is called once
    // after all Awake calls and before all any Update calls.
    public override void Start()
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

    public void OnEnable()
    {
        width = 0;
        height = 0;
    }

    // If this behaviour is enabled, OnGUI is called for rendering and handling GUI events.
    // It might be called several times per frame (one call per event).
    public void OnGUI()
    {
        if (width != Screen.width || height != Screen.height)
        {
            float x = alignRight
                          ? Screen.width * 0.98f - positionOffset.x - 50
                          : Screen.width * 0.02f + positionOffset.x;
            float y = alignBottom
                          ? Screen.height * 0.98f - positionOffset.y - 50
                          : Screen.height * 0.02f + positionOffset.y;
            width = Screen.width;
            height = Screen.height;
            windowRectangle = new Rect(x, y, 45, 40);
        }

        windowRectangle = GUI.Window(windowId, windowRectangle, WindowFunction, windowTitle);
    }

    // This creates the GUI inside the window.
    // It requires the id of the window it's currently making GUI for.
    private void WindowFunction(int windowID)
    {
        // Draw any Controls inside the window here.

        GUI.Label(new Rect(10, 10, 60, 20), fps.ToString("f1"));

        // Make the windows be draggable.
        GUI.DragWindow();
    }
}