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
   /// <summary>
    /// The base class for events.
    /// </summary>
    /// <typeparam name="T">
    /// The type of event data.
    /// </typeparam>
    public sealed class Event<T> : Event
    {
		/// <summary>
        /// Initializes a new instance of the Event class.
        /// </summary>
        /// <param name="eventId">
        /// The event ID.
        /// </param>
        /// <param name="eventType">
        /// The event type.
        /// </param>
        /// <param name="lifespan">
        /// The maximum duration of the event.
        /// </param>
        /// <param name="dispatchTime">
        /// The time to dispatch the event (or DISPATCH_IMMEDIATELY).
        /// </param>
        /// <param name="senderId">
        /// The sender ID (or SENDER_ID_IRRELEVANT).
        /// </param>
        /// <param name="receiverId">
        /// The receiver ID (or RECEIVER_ID_IRRELEVANT).
        /// </param>
        /// <param name="eventDelegate">
        /// The delegate to call when the event is triggered.
        /// </param>
        /// <param name="eventData">
        /// The event data.
        /// </param>
        private Event(
            int eventId,
            EventType eventType,
            Lifespan lifespan,
            double dispatchTime,
            int senderId,
            int receiverId,
            EventDelegate<T> eventDelegate,
            T eventData)
            : base(eventId, eventType, lifespan, dispatchTime, senderId, receiverId, eventDelegate, typeof(T), eventData){
        }
		
		 private Event(){
        }

        /// <summary>
        /// Gets the event data (may be null).
        /// </summary>
        public new T EventData
        {
            get {
                return (T)base.EventData;
            }

            private set {
                base.EventData = value;
            }
        }

        /// <summary>
        /// Gets the delegate to call when the event is triggered.
        /// </summary>
        public new EventDelegate<T> EventDelegate
        {
            get {
                return (EventDelegate<T>)base.EventDelegate;
            }

            private set {
                base.EventDelegate = value;
            }
        }
		
		public static Event<T> Obtain(
            int eventId,
            EventType eventType,
            Lifespan lifespan,
            double dispatchTime,
            int senderId,
            int receiverId,
            EventDelegate<T> eventDelegate,
            T eventData){
			// TODO: make events pool-able to reduce garbage
            return new Event<T>(
                    eventId,
                    eventType,
                    lifespan,
                    dispatchTime,
                    senderId,
                    receiverId,
                    eventDelegate,
                    eventData);
		}
		
		/// <summary>
        /// Returns a System.String that represents the event.
        /// </summary>
        /// <returns>A System.String that represents the event.</returns>
        public override string ToString(){
            return string.Format(
                "Id:{0}, Type:{1}, Lifespan:{2} Sender:{3}, Receiver:{4}, Data:{5}",
                EventId,
                EventType,
                EventLifespan,
                SenderId,
                ReceiverId,
                EventData);
        }

        /// <summary>
        /// Trigger event.
        /// </summary>
        /// <param name="delegateToFire">
        /// The event delegate to fire.
        /// </param>
        internal override void Fire(System.Delegate delegateToFire){
            var eventDelegate = delegateToFire as EventDelegate<T>;
            if (eventDelegate != null){
                eventDelegate(this);
            }
        }

        internal override void Send(){
			if (ReceiverId != EventManager.RECEIVER_ID_IRRELEVANT){
				Entity entity = EntityManager.Find<Entity>(ReceiverId);
				if (entity != null){
					entity.HandleEvent(this);
				}
			}
        }
	}
}

