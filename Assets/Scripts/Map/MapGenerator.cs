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

using System.Collections.Generic;

using Thot.GameAI;

using UnityEngine;

public sealed class MapGenerator : MonoBehaviour
{
	public bool generateMapOnAwake = true;
	public int cellWidth = 1;
	public int cellHeight = 1;
	public GameObject wallTemplate;
	
	private const float WALL_HEIGHT = 1f;
	private const float WALL_THICKNESS = 0.1f;
	private int columns;
	private int rows;
	
	public enum MapTypes
	{
		Maze,
		ConnectedRooms
	}
	
	private static MapGenerator _instance;

    public static MapGenerator Instance
    {
        get
        {
			return _instance;
        }
    }
	
	public void Awake()
	{
		if (_instance != null)
		{
			Debug.Log("Multiple instances of Map Generator!");
		}
		
		_instance = this;
		
		columns = (int)World.Instance.Size.x / cellWidth;
		rows = (int)World.Instance.Size.y / cellHeight;
		if (generateMapOnAwake)
		{
			GenerateMap();
		}
	}

	public void LoadMap(string filename)
    {
        switch (World.Instance.mapType)
        {
            case MapTypes.ConnectedRooms:
                LoadConnectedRoomsMap(filename);
                break;
            case MapTypes.Maze:
                LoadMazeMap(filename);
                break;
        }
    }

    public void GenerateMap()
    {
        switch (World.Instance.mapType)
        {
            case MapTypes.ConnectedRooms:
                GenerateConnectedRoomsMap();
                break;
            case MapTypes.Maze:
                GenerateMazeMap();
                break;
        }
    }
	
	private void LoadConnectedRoomsMap(string filename)
    {
        //// TODO
    }

    private void LoadMazeMap(string filename)
    {
        //// TODO
    }

    private void GenerateConnectedRoomsMap()
    {	
		GameObject wallsObject = GameObject.Find("Environment/Walls");
		//GameObject waypointsObject = GameObject.Find("Game/Waypoints");
		
        var connectedRooms = new ConnectedRooms(columns, rows);
        connectedRooms.BuildMap();

        connectedRooms.AddPointsOfVisibilityWaypoints();
        connectedRooms.OutputMap("ConnectedRooms.txt");
        ////ConnectedRooms.SaveMap("ConnectedRooms.xml");

        var wallScale = new Vector3(cellWidth, WALL_HEIGHT, cellHeight);

        //const float WAYPOINT_RADIUS = 1.0f; // entity size
		World.Instance.Waypoints = new List<Vector3>();

        for (int column = 0; column < connectedRooms.Columns; column++)
        {
            for (int row = 0; row < connectedRooms.Rows; row++)
            {
                if (connectedRooms.Map[column, row] == (int)ConnectedRooms.MapElements.Wall)
                {
                    var wallPosition =
                        new Vector3(
                            World.Instance.Center.x - World.Instance.Size.x / 2 + column * cellWidth + cellWidth / 2.0f,
                            transform.position.y + transform.localScale.y / 2.0f,
                            World.Instance.Center.y - World.Instance.Size.y / 2 + row * cellHeight + cellHeight / 2.0f);
					
					GameObject wall = Instantiate(wallTemplate, wallPosition, Quaternion.identity) as GameObject;	
					wall.transform.localScale = wallScale;
					wall.name = "Wall_" + row + "_" + column;
					wall.transform.parent = wallsObject.transform;
                }
                else if (connectedRooms.Map[column, row] == (int)ConnectedRooms.MapElements.Waypoint)
                {
                    var waypointPosition =
                        new Vector3(
                            World.Instance.Center.x - World.Instance.Size.x / 2 + column * cellWidth + cellWidth / 2.0f,
                            transform.position.y, // + transform.localScale.y / 2.0f,
                            World.Instance.Center.y - World.Instance.Size.y / 2 + row * cellHeight + cellHeight / 2.0f);
					
					World.Instance.Waypoints.Add(waypointPosition);
					
//					var waypoint =  GameObject.CreatePrimitive(PrimitiveType.Sphere);
//					Destroy(waypoint.collider);
//					waypoint.transform.position = waypointPosition;
//					waypoint.transform.localScale = new Vector3(WAYPOINT_RADIUS, WAYPOINT_RADIUS, WAYPOINT_RADIUS);
//					waypoint.name = "povWaypoint_" + row + "_" + column;
//					waypoint.collider.enabled = false;
//					waypoint.renderer.enabled = true;
//					waypoint.transform.parent = waypointsObject.transform;
                }
            }
        }
		
		wallScale.y *= 3;
		
		for (int column = -1; column < connectedRooms.Columns + 1; column++)
        {
            for (int row = -1; row < connectedRooms.Rows + 1; row++)
            {
				if (column != -1 && column != connectedRooms.Columns &&
				    row != -1 && row != connectedRooms.Rows)
				{
					continue;
				}
				
				var wallPosition =
                        new Vector3(
                            World.Instance.Center.x - World.Instance.Size.x / 2 + column * cellWidth + cellWidth / 2.0f,
                            transform.position.y + transform.localScale.y / 2.0f,
                            World.Instance.Center.y - World.Instance.Size.y / 2 + row * cellHeight + cellHeight / 2.0f);
				
				GameObject wall = Instantiate(wallTemplate, wallPosition, Quaternion.identity) as GameObject;	
				wall.transform.localScale = wallScale;
				wall.name = "Wall_" + row + "_" + column;
				wall.transform.parent = wallsObject.transform;
			}
		}
    }

    private void GenerateMazeMap()
    {	
		GameObject wallsObject = GameObject.Find("Environment/Walls");
		var maze = new Maze(rows, columns);
		maze.CreateMaze();
        maze.OutputMap("Maze.txt");
        ////Maze.SaveMap("Maze.xml");
		
		var horizontalWallScale = new Vector3(cellWidth, WALL_HEIGHT, WALL_THICKNESS);
        var verticalWallScale = new Vector3(WALL_THICKNESS, WALL_HEIGHT, cellHeight);
		
		GameObject horizontalWall;
		GameObject verticalWall;
		
		Vector3 thicknessAdjustment;
		Vector3 lengthAdjustment;
		
		for (int j = 0; j < columns; j++)
        {
            var horizontalWallPosition =
                new Vector3(
                    World.Instance.Center.x - World.Instance.Size.x / 2 + j * cellWidth + cellWidth / 2.0f,
                    transform.position.y + transform.localScale.y / 2.0f,
                    World.Instance.Center.y - World.Instance.Size.y / 2);

            horizontalWall = Instantiate(wallTemplate, horizontalWallPosition, Quaternion.identity) as GameObject;	
			horizontalWall.transform.localScale = horizontalWallScale;
			horizontalWall.name = "Horizontal_Wall_-1_" + j;
			
			thicknessAdjustment = new Vector3(0, 0, -WALL_THICKNESS / 2);
			horizontalWall.transform.localScale += thicknessAdjustment;
			horizontalWall.transform.Translate(-thicknessAdjustment / 2);	
			
			horizontalWall.transform.parent = wallsObject.transform;
        }
		
		for (int i = 0; i < rows; i++)
        {
            var verticalWallPosition =
                new Vector3(
                    World.Instance.Center.x - World.Instance.Size.x / 2,
                    transform.position.y + transform.localScale.y / 2.0f,
                    World.Instance.Center.y - World.Instance.Size.y / 2 + i * cellHeight + cellHeight / 2.0f);

            verticalWall = Instantiate(wallTemplate, verticalWallPosition, Quaternion.identity) as GameObject;
			verticalWall.transform.localScale = verticalWallScale;
			verticalWall.name = "Vertical_Wall_" + i + "_-1";
			
			thicknessAdjustment = new Vector3(-WALL_THICKNESS / 2, 0, 0);
			verticalWall.transform.localScale += thicknessAdjustment;
			verticalWall.transform.Translate(-thicknessAdjustment / 2);
			
			if (i == 0 || maze.Labrynth[i - 1, 0].Bottom)
			{
				lengthAdjustment = new Vector3(0, 0, -WALL_THICKNESS / 2);
				verticalWall.transform.localScale += lengthAdjustment;
				verticalWall.transform.Translate(-lengthAdjustment / 2);
			}
	
			if (maze.Labrynth[i, 0].Bottom)
			{
				lengthAdjustment = new Vector3(0, 0, -WALL_THICKNESS / 2);
				verticalWall.transform.localScale += lengthAdjustment;
				verticalWall.transform.Translate(lengthAdjustment / 2);
			}
			
			verticalWall.transform.parent = wallsObject.transform;
			
            for (int j = 0; j < columns; j++)
            {
                if (maze.Labrynth[i, j].Bottom)
                {
                    var horizontalWallPosition =
						new Vector3(
							World.Instance.Center.x - World.Instance.Size.x / 2 + j * cellWidth + cellWidth / 2.0f,
                            transform.position.y + transform.localScale.y / 2.0f,
                            World.Instance.Center.y - World.Instance.Size.y / 2 + i * cellHeight + cellHeight);

					horizontalWall = Instantiate(wallTemplate, horizontalWallPosition, Quaternion.identity) as GameObject;
					horizontalWall.transform.localScale = horizontalWallScale;
					horizontalWall.name = "Horizontal_Wall_" + i + "_" + j;
					
					if (j != 0 && !maze.Labrynth[i, j - 1].Bottom)
					{
						lengthAdjustment = new Vector3(WALL_THICKNESS / 2, 0, 0);
						horizontalWall.transform.localScale += lengthAdjustment;
						horizontalWall.transform.Translate(-lengthAdjustment / 2);
					}
					
					if (j != columns - 1 && !maze.Labrynth[i, j + 1].Bottom)
					{
						lengthAdjustment = new Vector3(WALL_THICKNESS / 2, 0, 0);
						horizontalWall.transform.localScale += lengthAdjustment;
						horizontalWall.transform.Translate(lengthAdjustment / 2);
					}
					
					horizontalWall.transform.parent = wallsObject.transform;
                }

                if (maze.Labrynth[i, j].Right)
                {
					var verticalWallPosition2 =
						new Vector3(
							World.Instance.Center.x - World.Instance.Size.x / 2 + j * cellWidth + cellWidth,
							transform.position.y + transform.localScale.y / 2.0f,
							World.Instance.Center.y - World.Instance.Size.y / 2 + i * cellHeight + cellHeight / 2.0f);

					verticalWall = Instantiate(wallTemplate, verticalWallPosition2, Quaternion.identity) as GameObject;
					verticalWall.transform.localScale = verticalWallScale;
					verticalWall.name = "Vertical_Wall_" + i + "_" + j;
					
					if (j == columns - 1)
					{
						thicknessAdjustment = new Vector3(-WALL_THICKNESS / 2, 0, 0);
						verticalWall.transform.localScale += thicknessAdjustment;
						verticalWall.transform.Translate(thicknessAdjustment / 2);
					}
					
					if (i == 0)
					{
						lengthAdjustment = new Vector3(0, 0, -WALL_THICKNESS / 2);
						verticalWall.transform.localScale += lengthAdjustment;
						verticalWall.transform.Translate(-lengthAdjustment / 2);
					}
					else if (maze.Labrynth[i - 1, j].Bottom || (j != columns - 1 && maze.Labrynth[i - 1, j + 1].Bottom))
					{
						lengthAdjustment = new Vector3(0, 0, -WALL_THICKNESS / 2);
						verticalWall.transform.localScale += lengthAdjustment;
						verticalWall.transform.Translate(-lengthAdjustment / 2);
					}
			
					if (i == rows - 1)
					{
						lengthAdjustment = new Vector3(0, 0, -WALL_THICKNESS / 2);
						verticalWall.transform.localScale += lengthAdjustment;
						verticalWall.transform.Translate(lengthAdjustment / 2);
					}
					else if (maze.Labrynth[i, j].Bottom|| (j != columns - 1 && maze.Labrynth[i, j + 1].Bottom))
					{
						lengthAdjustment = new Vector3(0, 0, -WALL_THICKNESS / 2);
						verticalWall.transform.localScale += lengthAdjustment;
						verticalWall.transform.Translate(lengthAdjustment / 2);
					}
					
					verticalWall.transform.parent = wallsObject.transform;
				}
			}
		}	
	}
}
