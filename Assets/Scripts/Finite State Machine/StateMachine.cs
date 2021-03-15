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

    /// <summary>
    /// State machine class. Inherit from this class and create some
    /// states to give your agents FSM functionality.
    /// </summary>
    /// <typeparam name="T">
    /// The type of game object associated with this state machine.
    /// </typeparam>
    public class StateMachine<T>
    {
        /// <summary>
        /// Initializes a new instance of the StateMachine class.
        /// </summary>
        /// <param name="owner">
        /// The game object that owns this state machine.
        /// </param>
        public StateMachine(T owner)
        {
            Owner = owner;
            CurrentState = null;
            PreviousState = null;
            GlobalState = null;
        }

        /// <summary>
        /// Gets the current state.
        /// </summary>
        /// <returns></returns>
        public State<T> CurrentState { get; private set; }

        /// <summary>
        /// Gets the global state.
        /// </summary>
        /// <returns></returns>
        public State<T> GlobalState { get; private set; }

        /// <summary>
        /// Gets the owner of this state machine.
        /// </summary>
        public T Owner { get; private set; }

        /// <summary>
        /// Gets the previous state.
        /// </summary>
        /// <returns></returns>
        public State<T> PreviousState { get; private set; }

        /// <summary>
        /// Change to a new state.
        /// </summary>
        /// <param name="newState">
        /// The new state.
        /// </param>
        public void ChangeState(State<T> newState)
        {
            if (newState == null)
            {
                Debug.LogError("StateMachine.ChangeState: trying to assign null state to current");
                return;
            }

            // keep a record of the previous state
            PreviousState = CurrentState;

            // call the exit method of the existing state
            CurrentState.Exit(Owner);

            // change state to the new state
            CurrentState = newState;

            // call the entry method of the new state
            CurrentState.Enter(Owner);
        }

        /// <summary>
        /// Only ever used during debugging to grab the name of the current state.
        /// </summary>
        /// <returns>
        /// The name of the current state.
        /// </returns>
        public string GetNameOfCurrentState()
        {
            string s = CurrentState.GetType().Name;

            // TODO: need to check this (C# vs C++)
            // remove the 'class ' part from the front of the string
            if (s.Length > 5)
            {
                s = s.Substring(6);
            }

            return s;
        }

        /// <summary>
        /// Handle messages.
        /// </summary>
        /// <param name="msg">
        /// The message.
        /// </param>
        /// <returns>
        /// True if message was handled. Otherwise, false.
        /// </returns>
        public bool HandleMessage(Telegram msg)
        {
            // first see if the current state is valid and that it can handle
            // the message
            if (CurrentState != null && CurrentState.OnMessage(Owner, msg))
            {
                return true;
            }

            // if not, and if a global state has been implemented, send
            // the message to the global state
            return GlobalState != null && GlobalState.OnMessage(Owner, msg);
        }

        /// <summary>
        /// Tests if the current state's type is equal to the type of the class passed as a
        /// parameter.
        /// </summary>
        /// <param name="st">
        /// The class to test against.
        /// </param>
        /// <returns>
        /// True if the current state's type is equal to the type of the class passed as a
        /// parameter.
        /// </returns>
        public bool IsInState(State<T> st)
        {
            return CurrentState.GetType() == st.GetType();
        }

        /// <summary>
        /// Change state back to the previous state.
        /// </summary>
        public void RevertToPreviousState()
        {
            ChangeState(PreviousState);
        }

        /// <summary>
        /// Set the current state.
        /// </summary>
        /// <param name="s">
        /// The new current state.
        /// </param>
        public void SetCurrentState(State<T> s)
        {
            CurrentState = s;
        }

        /// <summary>
        /// Set the global state (which is executed on every update).
        /// </summary>
        /// <param name="s">
        /// The new global state.
        /// </param>
        public void SetGlobalState(State<T> s)
        {
            GlobalState = s;
        }

        /// <summary>
        /// Set the previous state.
        /// </summary>
        /// <param name="s">
        /// The new previous state.
        /// </param>
        public void SetPreviousState(State<T> s)
        {
            PreviousState = s;
        }

        /// <summary>
        /// Call this to update the FSM.
        /// </summary>
        public void Update()
        {
            // if a global state exists, call its execute method, else do nothing
            if (GlobalState != null)
            {
                GlobalState.Execute(Owner);
            }

            // same for the current state
            if (CurrentState != null)
            {
                CurrentState.Execute(Owner);
            }
        }
    }
}