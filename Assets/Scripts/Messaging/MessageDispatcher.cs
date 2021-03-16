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
    /// A message dispatcher. Manages messages of the type Telegram.
    /// Instantiated as a singleton.
    /// </summary>
    public sealed class MessageDispatcher
    {
        /// <summary>
        /// There's no extra information.
        /// </summary>
        public const int NO_ADDITIONAL_INFO = 0;

        /// <summary>
        /// The id of the sender is irrelevant (system generated).
        /// </summary>
        public const int SENDER_ID_IRRELEVANT = -1;

        /// <summary>
        /// Message should be sent without delay.
        /// </summary>
        public const float SEND_MSG_IMMEDIATELY = 0.0f;

        /// <summary>
        /// The singleton instance of the message dispatcher. TODO: This could be made a service.
        /// </summary>
        private static MessageDispatcher _instance;

        /// <summary>
        /// Prevents a default instance of the MessageDispatcher class from being created.
        /// </summary>
        private MessageDispatcher(){
            MessageQueue = new PriorityQueue<Telegram, float>();
            if (null != _instance){
                Debug.LogError("Singleton already created.");
                return;
            }

            _instance = this;
        }

        /// <summary>
        /// Gets the accessor for the <see cref="MessageDispatcher"/> singleton instance.
        /// </summary>
        public static MessageDispatcher Instance
        {
            get {
                if (null == _instance){
                    new MessageDispatcher();
                }

                if (null == _instance){
                    Debug.LogError("Singleton instance not set by constructor.");
                    return default(MessageDispatcher);
                }

                return _instance;
            }
        }

        /// <summary>
        /// Gets the priority queue. A set is used as the container for the delayed messages because
        /// of the benefit of automatic sorting and avoidance of duplicates. Messages are sorted by
        /// their dispatch time. TODO: actually, set was not used. Need to check this.
        /// </summary>
        public PriorityQueue<Telegram, float> MessageQueue { get; private set; }

        /// <summary>
        /// This method is utilized by <see cref="DispatchMsg"/> or
        /// <see cref="DispatchDelayedMessages"/>. This method calls the message handling member of
        /// the receiving entity, receiver, with the newly created telegram.
        /// </summary>
        /// <param name="receiver">
        /// The intended receiver of the telegram.
        /// </param>
        /// <param name="telegram">
        /// The telegram.
        /// </param>
        public void Discharge(Entity receiver, Telegram telegram){
            if (!receiver.HandleMessage(telegram)){
                // telegram could not be handled
            }
        }

        /// <summary>
        /// This method dispatches any telegrams with a timestamp that has
        /// expired. Any dispatched telegrams are removed from the queue.
        /// </summary>
        public void DispatchDelayedMessages(){
            // first get current time
            float currentTime = Time.time;

            // now peek at the queue to see if any telegrams need dispatching.
            // remove all telegrams from the front of the queue that have gone
            // past their sell by date
            while (MessageQueue.Count > 0 && (MessageQueue.Peek().Value.DispatchTime < currentTime) && (MessageQueue.Peek().Value.DispatchTime > 0)){
                // read the telegram from the front of the queue
                Telegram telegram = MessageQueue.Peek().Value;

                // find the recipient
                Entity receiver = EntityManager.Find<Entity>(telegram.Receiver);

                // send the telegram to the recipient
                Discharge(receiver, telegram);

                // remove it from the queue
                MessageQueue.Dequeue();
            }
        }

        /// <summary>
        /// Given a message, a receiver, a sender and any time delay, this method routes the message
        /// to the correct agent (if no delay) or stores in the message queue to be dispatched at
        /// the correct time.
        /// </summary>
        /// <param name="delay">
        /// The message delay.
        /// </param>
        /// <param name="senderId">
        /// The sender ID.
        /// </param>
        /// <param name="receiverId">
        /// The receiver ID.
        /// </param>
        /// <param name="msg">
        /// The message.
        /// </param>
        /// <param name="extraInfo">
        /// Extra message data.
        /// </param>
        public void DispatchMsg(
            float delay,
            int senderId,
            int receiverId,
            MessageTypes msg,
            object extraInfo){
            // get a pointer to the receiver
            Entity receiver = EntityManager.Find<Entity>(receiverId);

            // make sure the receiver is valid
            if (receiver == null){
                return;
            }

            // create the telegram
            var telegram = new Telegram(SEND_MSG_IMMEDIATELY, senderId, receiverId, msg, extraInfo);

            // if there is no delay, route telegram immediately
            if (delay <= SEND_MSG_IMMEDIATELY){
                // send the telegram to the recipient
                Discharge(receiver, telegram);
            }

                // else calculate the time when the telegram should be dispatched
            else {
                float currentTime = Time.time;

                telegram.DispatchTime = currentTime + delay;

                // and put it in the queue
                MessageQueue.Enqueue(telegram, telegram.DispatchTime);
            }
        }
    }
}