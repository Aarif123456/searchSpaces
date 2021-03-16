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
    using System;

    /// <summary>
    /// This defines a telegram. A telegram is a data structure that records information required to
    /// dispatch messages. Messages are used by game objects to communicate with each other.
    /// </summary>
    public class Telegram
    {
        /// <summary>
        /// The minimum amount of time required between messages to be considered unique.
        /// </summary>
        private const float SMALLEST_DELAY = 0.25f;

        /// <summary>
        /// Initializes a new instance of the Telegram class.
        /// </summary>
        public Telegram()
            : this(-1, int.MaxValue, int.MaxValue, (MessageTypes)(-1), null){
        }

        /// <summary>
        /// Initializes a new instance of the Telegram class.
        /// </summary>
        /// <param name="dispatchTime">
        /// The time the message is dispatched.
        /// </param>
        /// <param name="sender">
        /// The message sender.
        /// </param>
        /// <param name="receiver">
        /// The intended message receiver.
        /// </param>
        /// <param name="msg">
        /// The message.
        /// </param>
        public Telegram(float dispatchTime, int sender, int receiver, MessageTypes msg)
            : this(dispatchTime, sender, receiver, msg, null){
        }

        /// <summary>
        /// Initializes a new instance of the Telegram class.
        /// </summary>
        /// <param name="dispatchTime">
        /// The time the message is dispatched.
        /// </param>
        /// <param name="sender">
        /// The message sender.
        /// </param>
        /// <param name="receiver">
        /// The intended message receiver.
        /// </param>
        /// <param name="msg">
        /// The message.
        /// </param>
        /// <param name="extraInfo">
        /// Extra data to send with the message.
        /// </param>
        public Telegram(
            float dispatchTime,
            int sender,
            int receiver,
            MessageTypes msg,
            object extraInfo){
            DispatchTime = dispatchTime;
            Sender = sender;
            Receiver = receiver;
            Msg = msg;
            ExtraInfo = extraInfo;
        }

        //// NOTE: our port does not do this. Need to check.
        //// these telegrams will be stored in a priority queue. Therefore the >
        //// operator needs to be overloaded so that the MessageQueue can sort the telegrams
        //// by time priority. Notice how the times must be smaller than
        //// SMALLEST_DELAY apart before two Telegrams are considered unique.

        /// <summary>
        /// Gets or sets the time to dispatch the message. Messages can be dispatched immediately or
        /// delayed for a specified  amount of time. If a delay is necessary this field is stamped
        /// with the time the message should be dispatched.
        /// </summary>
        public float DispatchTime { get; set; }

        /// <summary>
        /// Gets or sets any additional information that may accompany the message.
        /// </summary>
        public object ExtraInfo { get; set; }

        /// <summary>
        /// Gets or sets the message itself.
        /// </summary>
        public MessageTypes Msg { get; set; }

        /// <summary>
        /// Gets or sets the intended receiver of this telegram.
        /// </summary>
        public int Receiver { get; set; }

        /// <summary>
        /// Gets or sets the game object that sent this telegram.
        /// </summary>
        public int Sender { get; set; }

        /// <summary>
        /// Message ordering.
        /// TODO: we didn't use this.
        /// </summary>
        /// <param name="t">
        /// The other telegram.
        /// </param>
        /// <returns>
        /// True if this telegram is earlier than the specified one.
        /// </returns>
        public bool IsEarlierThan(Telegram t){
            if (this == t){
                return false;
            }

            return DispatchTime < t.DispatchTime;
        }

        /// <summary>
        /// Messages that are considered the same.
        /// TODO: we didn't use this.
        /// </summary>
        /// <param name="t">
        /// The other telegram.
        /// </param>
        /// <returns>
        /// True if this telegram is considered the same as the specified one.
        /// </returns>
        public bool IsSameAs(Telegram t){
            return Math.Abs(DispatchTime - t.DispatchTime) < SMALLEST_DELAY && Sender == t.Sender &&
                   Receiver == t.Receiver && Msg == t.Msg;
        }

        /// <summary>
        /// Convert message to readable format.
        /// </summary>
        /// <returns>
        /// The message as a string.
        /// </returns>
        public override string ToString(){
            return "time: " + DispatchTime + "  Sender: " + Sender + "   Receiver: " +
                Receiver + "   Msg: " + Msg;
        }
    }
}