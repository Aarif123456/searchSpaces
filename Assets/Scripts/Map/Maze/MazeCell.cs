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
	using UnityEngine;
	
	public sealed class MazeCell 
	{
		private readonly Maze _maze;
        private readonly int _row;
        private readonly int _column;
		
		/// <summary>
        /// Initializes a new instance of the MazeCell class.
        /// </summary>
        /// <param name="row">The cell's row index.</param>
        /// <param name="column">The cell's column index.</param>
        /// <param name="maze">The maze that owns this cell.</param>
        public MazeCell(int row, int column, Maze maze)
        {
            _row = row;
            _column = column;
            Bottom = true;
            Right = true;
            IsConnected = false;
            _maze = maze;
        }

        /// <summary>
        /// Gets the cell above.
        /// </summary>
        public MazeCell UpCell
        {
            get { return _maze.Labrynth[_row - 1, _column]; }
        }
		
		/// <summary>
        /// Gets the cell to the left.
        /// </summary>
        public MazeCell LeftCell
        {
            get { return _maze.Labrynth[_row, _column - 1]; }
        }

        /// <summary>
        /// Gets the cell below.
        /// </summary>
        public MazeCell DownCell
        {
            get { return _maze.Labrynth[_row + 1, _column]; }
        }

        /// <summary>
        /// Gets the cell to the right.
        /// </summary>
        public MazeCell RightCell
        {
            get { return _maze.Labrynth[_row, _column + 1]; }
        }

        public bool Bottom { get; set; }

        public bool IsConnected { get; set; }

        public bool Right { get; set; }

        /// <summary>
        /// Determine whether it is possible to move to the up.
        /// </summary>
        /// <returns>True if it is possible to move to the up.</returns>
        public bool CanGoUp()
        {
            if (_row == 0)
            {
                return false;
            }

            return !UpCell.IsConnected;
        }

        /// <summary>
        /// Determine whether it is possible to move to the left.
        /// </summary>
        /// <returns>True if it is possible to move to the left.</returns>
        public bool CanGoLeft()
        {
            if (_column == 0)
            {
                return false;
            }

            return !LeftCell.IsConnected;
        }

        /// <summary>
        /// Determine whether it is possible to move to the down.
        /// </summary>
        /// <returns>True if it is possible to move to the down.</returns>
        public bool CanGoDown()
        {
            if (_row == _maze.Rows - 1)
            {
                return false;
            }

            return !DownCell.IsConnected;
        }

        /// <summary>
        /// Determine whether it is possible to move to the right.
        /// </summary>
        /// <returns>True if it is possible to move to the right.</returns>
        public bool CanGoRight()
        {
            if (_column == _maze.Columns - 1)
            {
                return false;
            }

            return !RightCell.IsConnected;
        }
	}
}
