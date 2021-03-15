

namespace Thot.GameAI
{
	using System.ComponentModel;

    public static partial class Events
    {
        [Description("PathRequest")]
        public static readonly EventType PathRequest = (EventType)Count++;
		
		[Description("PathReady")]
        public static readonly EventType PathReady = (EventType)Count++;
		
		[Description("FollowCompleted")]
        public static readonly EventType FollowCompleted = (EventType)Count++;
		
		[Description("FollowFailed")]
        public static readonly EventType FollowFailed = (EventType)Count++;
		
		[Description("TraversalCompleted")]
        public static readonly EventType TraversalCompleted = (EventType)Count++;
		
		[Description("TraversalFailed")]
        public static readonly EventType TraversalFailed = (EventType)Count++;
		
		[Description("Arrival")]
        public static readonly EventType Arrival = (EventType)Count++;
    }
}
