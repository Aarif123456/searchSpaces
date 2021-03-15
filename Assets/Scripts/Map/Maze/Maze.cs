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
	
	public sealed class Maze
	{
		public Maze(int rows, int cols)
        {
            Rows = rows;
            Columns = cols;
            Labrynth = new MazeCell[rows, cols];
        }

        /// <summary>
        /// Gets or sets the maze array.
        /// </summary>
        public MazeCell[,] Labrynth { get; set; }

        /// <summary>
        /// Gets the number of rows in the maze array.
        /// </summary>
        public int Rows { get; private set; }

        /// <summary>
        /// Gets the number of columns in the maze array.
        /// </summary>
        public int Columns { get; private set; }
		
		/// <summary>
        /// Create a maze.
        /// </summary>
        public void CreateMaze()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    Labrynth[i, j] = new MazeCell(i, j, this);
                }
            }

            // now, remove walls
            int numconnected = 1;
            Labrynth[0, 0].IsConnected = true;

            var a = new List<MazeCell> { Labrynth[0, 0] }; // the fringe
            MazeCell currentCell;
            while (numconnected < (Rows * Columns))
            {
                int r = Random.Range(0, a.Count);
                currentCell = (MazeCell)a[r];
                int count = 0;
                if (currentCell.CanGoUp() || currentCell.CanGoDown() ||
                    currentCell.CanGoLeft() || currentCell.CanGoRight())
                {
                    // we can knock down a wall
                    // count the number of ways we can go, and then pick a wall to knock down.
                    if (currentCell.CanGoUp())
                    {
                        count++;
                    }

                    if (currentCell.CanGoDown())
                    {
                        count++;
                    }

                    if (currentCell.CanGoLeft())
                    {
                        count++;
                    }

                    if (currentCell.CanGoRight())
                    {
                        count++;
                    }

                    int temp = Random.Range(0, count);
                    if (currentCell.CanGoUp())
                    {
                        temp--;
                        if (temp == -1)
                        {
                            currentCell.UpCell.IsConnected = true;
                            currentCell.UpCell.Bottom = false;
                            a.Add(currentCell.UpCell);
                            numconnected++;
                        }
                    }

                    if (currentCell.CanGoDown())
                    {
                        temp--;
                        if (temp == -1)
                        {
                            currentCell.DownCell.IsConnected = true;
                            currentCell.Bottom = false;
                            a.Add(currentCell.DownCell);
                            numconnected++;
                        }
                    }

                    if (currentCell.CanGoLeft())
                    {
                        temp--;
                        if (temp == -1)
                        {
                            currentCell.LeftCell.IsConnected = true;
                            currentCell.LeftCell.Right = false;
                            a.Add(currentCell.LeftCell);
                            numconnected++;
                        }
                    }

                    if (currentCell.CanGoRight())
                    {
                        temp--;
                        if (temp == -1)
                        {
                            currentCell.RightCell.IsConnected = true;
                            currentCell.Right = false;
                            a.Add(currentCell.RightCell);
                            numconnected++;
                        }
                    }
                }
                else // remove that MazeCell
                {
                    a.RemoveAt(r);
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
            //// TODO
        }

        /// <summary>
        /// Save a map to the specified filename.
        /// </summary>
        /// <param name="filename">The filename to savee to.</param>
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
            var stringBuilder = new StringBuilder();

            // column headers
            stringBuilder.Append("    ");
            for (int column = 0; column < Columns; column++)
            {
                if (column / 10 > 0)
                {
                    stringBuilder.Append(column / 10 + " ");
                }
                else
                {
                    stringBuilder.Append("  ");
                }
            }

            stringBuilder.Append(System.Environment.NewLine);
            stringBuilder.Append("    ");
            for (int column = 0; column < Columns; column++)
            {
                stringBuilder.Append(column % 10 + " ");
            }

            stringBuilder.Append(System.Environment.NewLine);

            stringBuilder.Append("   ");
            stringBuilder.Append(" ");
            for (int column = 0; column < Columns; column++)
            {
                stringBuilder.Append("_ ");
            }

            stringBuilder.Append(System.Environment.NewLine);

            // maze
            for (int row = 0; row < Rows; row++)
            {
                // number the rows
                if (row < 10)
                {
                    stringBuilder.Append(" ");
                }

                stringBuilder.Append(row + " ");
                stringBuilder.Append("|");
                for (int column = 0; column < Columns; column++)
                {
                    if (Labrynth[row, column].Bottom)
                    {
                        stringBuilder.Append("_");
                    }
                    else
                    {
                        stringBuilder.Append(" ");
                    }

                    if (Labrynth[row, column].Right)
                    {
                        stringBuilder.Append("|");
                    }
                    else
                    {
                        stringBuilder.Append(" ");
                    }
                }

                stringBuilder.Append(System.Environment.NewLine);
            }

            return stringBuilder.ToString();
        }
	}
}
