namespace EventBusManager
{
    public class EventSubscription
    {
        public bool IsDynamic { get; }
        public Type HandlerType { get; }

        private EventSubscription(bool isDynamic, Type handlerType)
        {
            IsDynamic = isDynamic;
            HandlerType = handlerType;
        }

        public static EventSubscription Dynamic(Type handlerType) =>
            new EventSubscription(true, handlerType);

        public static EventSubscription Typed(Type handlerType) =>
            new EventSubscription(false, handlerType);
    }
}