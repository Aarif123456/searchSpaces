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

namespace Thot.GameAI
{
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	
	using UnityEngine;
	
	/// <summary>
    /// A class for generating rooms connected by corridors in a map area partitioned into a grid of
    /// rows and columns.
    /// </summary>
	public sealed class ConnectedRooms
	{
		/// <summary>
        /// The random generator used in generating the map.
        /// </summary>
        /// <remarks>
        /// We use a separate random number generate (not the game engine random number generator)
        /// so we can set the seed. This allows us to be able to deterministically recreate maps for
        /// testing purposes. Setting the Seed property will reseed the generator.
        /// </remarks>
        private System.Random _random = new System.Random();
		
		/// <summary>
        /// The number of rooms that have been generated so far.
        /// </summary>
        private int _currentRoomCount;
		
		/// <summary>
        /// Initializes a new instance of the ConnectedRooms class with a map of size 
        /// <paramref name="rows">rows</paramref> by <paramref name="columns">columns</paramref>.
        /// </summary>
        /// <param name="rows">The number of rows in the map.</param>
        /// <param name="columns">The number of columns in the map.</param>
        public ConnectedRooms(int rows, int columns)
        {
            Rows = rows < 10 ? 10 : rows;
            Columns = columns < 10 ? 10 : columns;
            MapFilename = "ConnectedRooms.txt";
            MaximumRoomColumns = 7;
            MaximumRoomRows = 6;
            MinimumRoomColumns = 2;
            MinimumRoomRows = 2;
            MaximumRoomCount = 10;
            MaximumCorridorLength = 3;
            MinimumCorridorLength = 1;
            RoomOnCorridorGenerationProbability = 80;
            RoomOnCorridorEndGenerationProbability = 100;
            CorridorOnRoomEdgeGenerationProbability = 60;
            CorridorOnCorridorEndGenerationProbability = 0;
            CorridorOnCorridorGenerationProbability = 0;
            MaximumAttempts = 1000;
            ResetMap();
        }
		
		public enum MapElements
        {
            Wall = 0,
            Space = 1,
            Door = 2,
            Waypoint = 3,
        }

        private enum Directions
        {
            North = 0,
            East,
            South,
            West,
            First = North,
            Last = West
        }
		
		/// <summary>
        /// Sets the seed of the random generator.
        /// </summary>
        /// <remarks>
        /// Use this to seed the random number generator. This is intended to provide a means to
        /// ensure the same map can be regenerated for repeatability of experiments.
        /// </remarks>
        public int Seed
        {
            set
            {
                _random = new System.Random(value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of times to try operations that may be difficult or
        /// impossible.
        /// </summary>
        /// <remarks>
        /// This is used to limit the effort in trying to satisfy difficult (or even impossible)
        /// requirements. This ensures map generation will terminate.
        /// </remarks>
        public int MaximumAttempts { get; set; }
		
		/// <summary>
        /// Gets or sets the name of the file to output a character-based representation of the map.
        /// Set to null or string.Empty to disable output.
        /// </summary>
        public string MapFilename { get; set; }

        /// <summary>
        /// Gets the generated map.
        /// </summary>
        public int[,] Map { get; private set; }

        /// <summary>
        /// Gets the number of rows in the map.
        /// </summary>
        public int Rows { get; private set; }

        /// <summary>
        /// Gets the number of columns in the map.
        /// </summary>
        public int Columns { get; private set; }

        /// <summary>
        /// Gets the list of generated rooms.
        /// </summary>
        public List<Rect> Rooms { get; private set; }

        /// <summary>
        /// Gets the list of generated corridors.
        /// </summary>
        public List<Rect> Corridors { get; private set; }

        /// <summary>
        /// Gets the list of doors.
        /// </summary>
        public List<Vector2> Doors { get; private set; }
		
		/// <summary>
        /// Gets or sets the maximum number of rooms to generate.
        /// </summary>
        public int MaximumRoomCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of columns a generated room can span.
        /// </summary>
        public int MaximumRoomColumns { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of rows a generated room can span.
        /// </summary>
        public int MaximumRoomRows { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of columns a generated room can span.
        /// </summary>
        public int MinimumRoomColumns { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of rows a generated room can span.
        /// </summary>
        public int MinimumRoomRows { get; set; }

        /// <summary>
        /// Gets or sets the maximum length (in cells) of a generated corridor.
        /// </summary>
        public int MaximumCorridorLength { get; set; }

        /// <summary>
        /// Gets or sets the minimum length (in cells) of a generated corridor.
        /// </summary>
        public int MinimumCorridorLength { get; set; }

        /// <summary>
        /// Gets or sets the probability of choosing to generate a corridor emanating from another
        /// corridor.
        /// </summary>
        public int CorridorOnCorridorGenerationProbability { get; set; }

        /// <summary>
        /// Gets or sets the probability of choosing to generate a corridor emanating from the end
        /// of another corridor.
        /// </summary>
        public int CorridorOnCorridorEndGenerationProbability { get; set; }

        /// <summary>
        /// Gets or sets the probability of choosing to generate a corridor emanating from a room
        /// edge.
        /// </summary>
        public int CorridorOnRoomEdgeGenerationProbability { get; set; }

        /// <summary>
        /// Gets or sets the probability of choosing to generate a room at the end of a corridor.
        /// </summary>
        public int RoomOnCorridorEndGenerationProbability { get; set; }

        /// <summary>
        /// Gets or sets the probability of choosing to generate a room on a corridor.
        /// </summary>
        public int RoomOnCorridorGenerationProbability { get; set; }

        /// <summary>
        /// Reset the map.
        /// </summary>
        public void ResetMap()
        {
            Map = new int[Rows, Columns];
            Rooms = new List<Rect>(MaximumRoomCount);
            Corridors = new List<Rect>(MaximumRoomCount * 4);
            Doors = new List<Vector2>(MaximumRoomCount * 4);

            _currentRoomCount = 0;
        }

        /// <summary>
        /// Build a map.
        /// </summary>
        public void BuildMap()
        {
            // Build a room at a random available location.
            if (_currentRoomCount >= MaximumRoomCount || !BuildRoom(new Vector2(1, 1)))
            {
                return;
            }

            if (!BuildCorridorOnRoomEdge())
            {
                return;
            }

            for (int attempt = 0; attempt < MaximumAttempts; attempt++)
            {
                int roll = _random.Next(0, 100);

                if (roll < CorridorOnCorridorGenerationProbability)
                {
                    BuildCorridorOnCorridor();
                }
                
                if (roll < CorridorOnCorridorEndGenerationProbability)
                {
                    BuildCorridorOnCorridorEnd();
                }
                
                if (roll < CorridorOnRoomEdgeGenerationProbability)
                {
                    BuildCorridorOnRoomEdge();
                }

                if (_currentRoomCount < MaximumRoomCount && 
                    roll < RoomOnCorridorGenerationProbability)
                {
                    BuildRoomOnCorridor();
                }

                if (_currentRoomCount < MaximumRoomCount && 
                    roll < RoomOnCorridorEndGenerationProbability)
                {
                    BuildRoomOnCorridorEnd();
                }
            }
        }

        /// <summary>
        /// Add waypoints on both sides of every door.
        /// </summary>
        /// <remarks>
        /// NOTE: This does not correctly handle corridor-corridor connections (no doors!).
        /// TODO: Fix!
        /// </remarks>
        public void AddPointsOfVisibilityWaypoints()
        {
            foreach (Vector2 door in Doors)
            {
                var column = (int)door.x;
                var row = (int)door.y;
                if (Map[column, row] != (int)MapElements.Door)
                {
                    continue;
                }

                if (column > 0 && Map[column - 1, row] == (int)MapElements.Space)
                {
                    Map[column - 1, row] = (int)MapElements.Waypoint;
                }

                if (column < Columns - 1 && Map[column + 1, row] == (int)MapElements.Space)
                {
                    Map[column + 1, row] = (int)MapElements.Waypoint;
                }

                if (row > 0 && Map[column, row - 1] == (int)MapElements.Space)
                {
                    Map[column, row - 1] = (int)MapElements.Waypoint;
                }

                if (row < Map.GetLength(1) - 1 && Map[column, row + 1] == (int)MapElements.Space)
                {
                    Map[column, row + 1] = (int)MapElements.Waypoint;
                }
            }
        }

        public void AddCornerWayPoints()
        {
            for (int row = 1; row < Rows-1; row++)
            {
                for (int column = 1; column < Columns-1; column++)
                {
                    if(Map[column, row] == (int)MapElements.Wall){
                        if( (Map[column+1, row] == (int)MapElements.Space || Map[column+1, row] == (int)MapElements.Door) 
                            && (Map[column+1, row+1] == (int)MapElements.Space || Map[column+1, row+1] == (int)MapElements.Door)
                            && (Map[column, row+1] == (int)MapElements.Space || Map[column, row+1] == (int)MapElements.Door) 
                          )
                        {
                             Map[column+1, row +1] = (int)MapElements.Waypoint;
                        }
                        else if( (Map[column-1, row] == (int)MapElements.Space  || Map[column-1, row] == (int)MapElements.Door)
                            && (Map[column-1, row-1] == (int)MapElements.Space  || Map[column-1, row-1] == (int)MapElements.Door)
                            && (Map[column, row-1] == (int)MapElements.Space  || Map[column, row-1] == (int)MapElements.Door)
                          )
                        {
                            Map[column-1, row -1] = (int)MapElements.Waypoint;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Output a character-based representation of the map.
        /// </summary>
        /// <param name="filename">The filename to output to.</param>
        public void OutputMap(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return;
            }

            StreamWriter streamWriter;

            try
            {
                streamWriter = new StreamWriter(filename);
                streamWriter.WriteLine(this);
                streamWriter.Close();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error outputting map: " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Load a map from the specified filename.
        /// </summary>
        /// <param name="filename">The filename to load from.</param>
        public void LoadMap(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return;
            }

            try
            {
                ResetMap();
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(filename))
                {
                    string line;
                    // Read and display lines from the file until the end of
                    // the file is reached.
                    for (int row = 0; (line = sr.ReadLine()) != null && row < Rows; row++)
                    {
                        int column=0;
                        foreach(char c in line){
                            switch (c)
                            {
                                case 'w': // wall
                                    Map[column, row] = (int)MapElements.Wall;
                                    break;
                                case ' ': // space
                                    Map[column, row] = (int)MapElements.Space;
                                    break;
                                case 'd': //door
                                    Map[column, row] = (int)MapElements.Door;
                                    break;
                                case 'p': // pov waypoint
                                    Map[column, row] = (int)MapElements.Waypoint;
                                    break;
                                default:
                                    Debug.Log("\"" +c +"\" is not a recognized character");
                                    break;
                            }
                            column++;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
               Debug.LogError("Error loading map: " + e.Message);
               throw;
            }
        }

        /// <summary>
        /// Save a map to the specified filename.
        /// </summary>
        /// <param name="filename">The filename to save to.</param>
        public void SaveMap(string filename)
        {
           //// TODO
        }

        /// <summary>
        /// Create a string representation of the map.
        /// </summary>
        /// <returns>A string representation of the map.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder((Columns + 2) * Rows);

            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    switch (Map[column, row])
                    {
                        case (int)MapElements.Wall: // wall
                            stringBuilder.Append("w");
                            break;
                        case (int)MapElements.Space: // space
                            stringBuilder.Append(" ");
                            break;
                        case (int)MapElements.Door: // door
                            stringBuilder.Append("d");
                            break;
                        case (int)MapElements.Waypoint: // pov waypoint
                            stringBuilder.Append("p");
                            break;
                    }
                }

                stringBuilder.AppendFormat(System.Environment.NewLine);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Attempt to build a room at (1, 1).
        /// </summary>
        /// <param name="desiredRoomLocation">
        /// The desired room location.
        /// </param>
        /// <remarks>
        /// First this randomly chooses a valid room size.
        /// Then it attempts to place a room at the location (shrinking if needed).
        /// </remarks>
        /// <returns>
        /// True if a room was placed. Otherwise false.
        /// </returns>
        private bool BuildRoom(Vector2 desiredRoomLocation)
        {
            var desiredRoomSize = new Vector2(
                _random.Next(MinimumRoomColumns, MaximumRoomColumns), 
                _random.Next(MinimumRoomRows, MaximumRoomRows));

            var desiredRectangle = 
				new Rect(
					desiredRoomLocation.x, 
				         desiredRoomLocation.y, 
				         desiredRoomSize.x,
				         desiredRoomSize.y);
            Rect fitRectangle;

            if (FitAndBuildRoomRectangle(desiredRectangle, out fitRectangle))
            {
                Rooms.Add(fitRectangle);
                _currentRoomCount++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempt to build a room.
        /// </summary>
        /// <remarks>
        /// First this randomly chooses a valid room size.
        /// Next, it chooses a random location.
        /// Then it attempts to place a room at the location (shrinking if needed).
        /// If it does not fit, another size and location is chosen. (The number of attempts is limited).
        /// </remarks>
        /// <returns>True if a room was placed. Otherwise false.</returns>
        private bool BuildRoom()
        {
            for (int attempt = 0; attempt < MaximumAttempts; attempt++)
            {
                var desiredRoomSize = 
                    new Vector2(
                        _random.Next(MinimumRoomColumns, MaximumRoomColumns), 
                        _random.Next(MinimumRoomRows, MaximumRoomRows));

                var desiredRoomLocation = 
                    new Vector2(
                        _random.Next(0, Columns - (int)desiredRoomSize.x), 
                        _random.Next(0, Rows - (int)desiredRoomSize.y));

                var desiredRectangle = 
					new Rect(
					         desiredRoomLocation.x, 
					         desiredRoomLocation.y,
					         desiredRoomSize.x,
					         desiredRoomSize.y);
                Rect fitRectangle;

                if (FitAndBuildRoomRectangle(desiredRectangle, out fitRectangle))
                {
                    Rooms.Add(fitRectangle);
                    _currentRoomCount++;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempt to fit the given rectangle (shrinking if necessary) then building the
        /// rectangle on the map.
        /// </summary>
        /// <param name="rectangle">The initial rectangle to fit and build.</param>
        /// <param name="currentRectangle">The final fitted rectangle.</param>
        /// <returns>True if the rectangle was fit and built. Otherwise, false.</returns>
        private bool FitAndBuildRoomRectangle(Rect rectangle, out Rect currentRectangle)
        {
            currentRectangle = rectangle;

            for (int width = (int)rectangle.width; width >= MinimumRoomColumns; width--)
            {
                currentRectangle.width = width;
                for (int height = (int)rectangle.height; height >= MinimumRoomRows; height--)
                {
                    currentRectangle.height = height;

                    if (RectangleFits(currentRectangle))
                    {
                        BuildRectangle(currentRectangle);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Test if the rectangle area is unoccupied (contains no open cells).
        /// </summary>
        /// <param name="rectangle">The rectangle to test.</param>
        /// <returns>True if the rectangle fits.</returns>
        private bool RectangleFits(Rect rectangle)
        {
            // Inflate the rectangle, to ensure no overlaps occur
            if (rectangle.width == 0) // is corridor
            {
                rectangle = Inflate(rectangle, 1, 0);
            }
            else if (rectangle.height == 0) // is corridor
            {
                rectangle = Inflate(rectangle, 0, 1);
            }
            else // is room
            {
                rectangle = Inflate(rectangle, 1, 1);
            }

            for (int row = (int)rectangle.x; row <= rectangle.xMax; row++)
            {
                if (row < 0 || row >= Rows)
                {
                    return false;
                }

                for (int column = (int)rectangle.yMin; column <= rectangle.yMax; column++)
                {
                    if (column < 0 || column >= Columns || Map[row, column] != (int)MapElements.Wall)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Place the rectangle in the map. This zeros out the cells overlapped by the rectangle.
        /// </summary>
        /// <remarks>
        /// This does not test if the rectangle fits in the map. The fit should be tested before
        /// calling this method.
        /// </remarks>
        /// <param name="rectangle">The rectangle to place in the map.</param>
        private void BuildRectangle(Rect rectangle)
        {
            for (int row = (int)rectangle.xMin; row <= rectangle.xMax; row++)
            {
                for (int column = (int)rectangle.yMin; column <= rectangle.yMax; column++)
                {
                    Map[row, column] = (int)MapElements.Space;
                }
            }
        }

        /// <summary>
        /// Attempt to build a corridor on the side of a room.
        /// </summary>
        /// <returns>
        /// True if a corridor was built. Otherwise, false.
        /// </returns>
        private bool BuildCorridorOnRoomEdge()
        {
            for (int attempt = 0; attempt < MaximumAttempts; attempt++)
            {
                int direction;
                Vector2 door = GetRoomEdge(out direction);

                Rect corridor = GetCorridor(door, direction);
                if (RectangleFits(corridor))
                {
                    Corridors.Add(corridor);
                    BuildRectangle(corridor);
                    Map[(int)door.x, (int)door.y] = (int)MapElements.Door;
                    Doors.Add(door);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get a position of a random room's wall.
        /// </summary>
        /// <param name="directionAwayFromRoom">
        /// Direction of wall, e.g. 0 for north (top),  1 for east (right) etc.
        /// </param>
        /// <returns>Point containing location.</returns>
        private Vector2 GetRoomEdge(out int directionAwayFromRoom)
        {
            Rect room = Rooms[_random.Next(0, Rooms.Count)];
            directionAwayFromRoom = GetDirection();
            var pointOnWallOfRoom = new Vector2();

            // Boundary of the inflated rectangle will contain the pointOnWallOfRoom.
            room = Inflate(room, 1, 1);

            switch (directionAwayFromRoom)
            {
                case (int)Directions.North:
                    pointOnWallOfRoom = 
                        new Vector2(_random.Next((int)room.xMin + 1, (int)room.xMax), room.yMin);
                    break;
                case (int)Directions.East:
                    pointOnWallOfRoom = 
                        new Vector2(room.xMax, _random.Next((int)room.yMin + 1, (int)room.yMax));
                    break;
                case (int)Directions.South:
                    pointOnWallOfRoom = 
                        new Vector2(_random.Next((int)room.xMin + 1, (int)room.xMax), room.yMax);
                    break;
                case (int)Directions.West:
                    pointOnWallOfRoom = 
                        new Vector2(room.xMin, _random.Next((int)room.yMin + 1, (int)room.yMax));
                    break;
            }

            return pointOnWallOfRoom;
        }

        /// <summary>
        /// Get a random direction.
        /// </summary>
        /// <returns>A random direction (North, East, South or West).</returns>
        private int GetDirection()
        {
            // North, East, South or West
            return _random.Next((int)Directions.First, (int)Directions.Last);
        }

        /// <summary>
        /// Get a rectangle representing a corridor joined to the connection point and running in
        /// the given direction.
        /// </summary>
        /// <param name="connectionPoint">The connection point (door or corridor endpoint).</param>
        /// <param name="direction">The corridor direction.</param>
        /// <returns>A rectangle representing a corridor.</returns>
        private Rect GetCorridor(Vector2 connectionPoint, int direction)
        {
            int length = GetCorridorLength();
            var corridor = new Rect(connectionPoint.x, connectionPoint.y, 0, 0);

            switch (direction)
            {
                case (int)Directions.North:
                    corridor.y -= length + 1;
                    corridor.height = length;
                    break;
                case (int)Directions.East:
                    corridor.x += 1;
                    corridor.width = length;
                    break;
                case (int)Directions.South:
                    corridor.y += 1;
                    corridor.height = length;
                    break;
                case (int)Directions.West:
                    corridor.x -= length + 1;
                    corridor.width = length;
                    break;
            }

            return corridor;
        }

        /// <summary>
        /// Get a valid random length for a corridor.
        /// </summary>
        /// <returns>A length for a corridor.</returns>
        private int GetCorridorLength()
        {
            return _random.Next(MinimumCorridorLength, MaximumCorridorLength);
        }

        /// <summary>
        /// Attempt to build a room at the end of a corridor.
        /// </summary>
        /// <returns>True if room built at the end of a corridor.</returns>
        private bool BuildRoomOnCorridorEnd()
        {
            for (int attempt = 0; attempt < MaximumAttempts; attempt++)
            {
                int directionAwayFromCorridor;
                Vector2 corridorEndpoint = GetEndOfCorridor(out directionAwayFromCorridor);
                Vector2 door;

                Rect room = GetRoom(corridorEndpoint, directionAwayFromCorridor, out door);

                if (RectangleFits(room))
                {
                    BuildRectangle(room);
                    Rooms.Add(room);
                    Doors.Add(door);
                    Map[(int)door.x, (int)door.y] = (int)MapElements.Door;
                    _currentRoomCount++;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the end point of a random corridor.
        /// </summary>
        /// <param name="direction">The direction away from the corridor.</param>
        /// <returns>The end point of a corridor.</returns>
        private Vector2 GetEndOfCorridor(out int direction)
        {
            Rect corridor = Corridors[_random.Next(0, Corridors.Count)];
            Vector2 endPoint = new Vector2(corridor.x, corridor.y);

            if (corridor.height > 0)
            {
                // Randomly switch to other end of the corridor.
                if (_random.Next(0, 2) == 0)
                {
                    direction = (int)Directions.North;   
                    endPoint.y += corridor.height;
                }
                else
                {
                    direction = (int)Directions.South;
                }          
            }
            else if (corridor.width > 0)
            {
                // Randomly switch to other end of the corridor.
                if (_random.Next(0, 2) == 0)
                {
                    direction = (int)Directions.East;
                    endPoint.x += corridor.width;
                }
                else
                {
                    direction = (int)Directions.West;
                }
            }
            else
            {
                direction = 0; // invalid!
                Debug.LogError(string.Format("Invalid corridor [{0}]", corridor));
                throw new System.Exception(string.Format("Invalid corridor [{0}]", corridor));
            }

            return endPoint;
        }

        /// <summary>
        /// Get a rectangle representing a room using the provided values.
        /// </summary>
        /// <param name="connectionPoint">The connection point (part of the adjacent room or corridor).</param>
        /// <param name="direction">The direction to the room from the starting point.</param>
        /// <param name="door">The door location (once cell from the connection point in the given direction).</param>
        /// <returns>A rectangle representing a room.</returns>
        private Rect GetRoom(Vector2 connectionPoint, int direction, out Vector2 door)
        {
            var location = new Vector2();
            door = connectionPoint;

            var size =
                new Vector2(
                    _random.Next(MinimumRoomColumns, MaximumRoomColumns),
                    _random.Next(MinimumRoomRows, MaximumRoomRows));

            switch (direction)
            {
                case (int)Directions.North:
                    door = Offset(door, 0, -1);
                    connectionPoint= Offset(connectionPoint, 0, -2);
                    location = new Vector2(
                        connectionPoint.x - _random.Next(1, (int)size.x),
                        connectionPoint.y - size.y);
                    break;
                case (int)Directions.East:
                    door = Offset(door, 1, 0);
                    connectionPoint = Offset(connectionPoint, 2, 0);
                    location = new Vector2(
                        connectionPoint.x,
                        connectionPoint.y - _random.Next(1, (int)size.y));
                    break;
                case (int)Directions.South:
                    door= Offset(door, 0, 1);
                    connectionPoint = Offset(connectionPoint, 0, 2);
                    location = new Vector2(
                        connectionPoint.x - _random.Next(1, (int)size.x),
                        connectionPoint.y);
                    break;
                case (int)Directions.West:
                    door = Offset(door, -1, 0);
                    connectionPoint = Offset(connectionPoint, -2, 0);
                    location = new Vector2(
                        connectionPoint.x - size.x,
                        connectionPoint.y - _random.Next(1, (int)size.y));
                    break;
            }

            return new Rect(location.x, location.y, size.x, size.y);
        }

        /// <summary>
        /// Attempt to build a room off a random point on a corridor.
        /// </summary>
        /// <returns>
        /// True if the a room is built on the corridor. Otherwise, false.
        /// </returns>
        private bool BuildRoomOnCorridor()
        {
            //// the offest of start is to move one point in the direction
            //// the room is being built, to ensure it has a discrete entry point

            for (int attempt = 0; attempt < MaximumAttempts; attempt++)
            {
                int directionAwayFromCorridor;
                Vector2 corridorPoint = GetCorridorPoint(out directionAwayFromCorridor);

                Vector2 door;

                Rect room = GetRoom(corridorPoint, directionAwayFromCorridor, out door);

                if (RectangleFits(room))
                {
                    BuildRectangle(room);
                    Rooms.Add(room);
                    Doors.Add(door);
                    Map[(int)door.x, (int)door.y] = (int)MapElements.Door;
                    _currentRoomCount++;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get a random point on a random corridor.
        /// </summary>
        /// <param name="directionAwayFromCorridor">
        /// A direction away from the corridor.
        /// </param>
        /// <returns>
        /// A point on a corridor.
        /// </returns>
        private Vector2 GetCorridorPoint(out int directionAwayFromCorridor)
        {
            Rect corridor = Corridors[_random.Next(0, Corridors.Count)];

            if (corridor.height > 0)
            {
                // Randomly pick a direction perpendicular to the corridor
                if (_random.Next(0, 2) == 0)
                {
                    directionAwayFromCorridor = (int)Directions.East;
                }
                else
                {
                    directionAwayFromCorridor = (int)Directions.West;
                }
            }
            else if (corridor.width > 0)
            {
                // Randomly pick a direction perpendicular to the corridor
                if (_random.Next(0, 2) == 0)
                {
                    directionAwayFromCorridor = (int)Directions.North;
                }
                else
                {
                    directionAwayFromCorridor = (int)Directions.South;
                }
            }
            else
            {
                directionAwayFromCorridor = 0; // invalid!
                Debug.LogError(string.Format("Invalid corridor [{0}]", corridor));
                throw new System.Exception(string.Format("Invalid corridor [{0}]", corridor));
            }

            return 
                new Vector2(
                    _random.Next((int)corridor.xMin, (int)corridor.xMax),
                    _random.Next((int)corridor.yMin, (int)corridor.yMax));
        }

        /// <summary>
        /// Attempt to build a corridor at a random point on a random corridor.
        /// </summary>
        /// <returns>True if a corridor was built. Otherwise, false.</returns>
        private bool BuildCorridorOnCorridor()
        {
            for (int attempt = 0; attempt < MaximumAttempts; attempt++)
            {
                int directionAwayFromCorridor;
                Vector2 corridorPoint = GetCorridorPoint(out directionAwayFromCorridor);

                Rect corridor = GetCorridor(corridorPoint, directionAwayFromCorridor);
                if (RectangleFits(corridor))
                {
                    Corridors.Add(corridor);
                    BuildRectangle(corridor);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempt to build a corridor at the end of a random corridor.
        /// </summary>
        /// <returns>
        /// True if a corridor was built. Otherwise, false.
        /// </returns>
        private bool BuildCorridorOnCorridorEnd()
        {
            for (int attempt = 0; attempt < MaximumAttempts; attempt++)
            {
                int directionAwayFromCorridor;
                Vector2 corridorEndpoint = GetEndOfCorridor(out directionAwayFromCorridor);

                Rect corridor = GetCorridor(corridorEndpoint, directionAwayFromCorridor);
                if (RectangleFits(corridor))
                {
                    Corridors.Add(corridor);
                    BuildRectangle(corridor);
                    return true;
                }
            }

            return false;
        }
		
		private Rect Inflate(Rect rectangle, int width, int height)
        {
            rectangle.x -= width;
            rectangle.y -= height;
            rectangle.width += 2 * width;
            rectangle.height += 2 * height;
			return rectangle;
        }
		
		private Vector2 Offset(Vector2 point, int dx, int dy)
        {
            point.x += dx;
            point.y += dy;
			return point;
        }
	}
}