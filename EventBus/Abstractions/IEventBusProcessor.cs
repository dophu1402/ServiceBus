namespace EventBusManager.Abstractions
{
    public interface IBaseEvenBusProcessor
    {
        Task RegisterSubscriptionClientMessageHandlerAsync();
    }
    public interface IEventBusProcessor : IBaseEvenBusProcessor
    {
    }

    public interface ISessionEventBusProcessor : IBaseEvenBusProcessor
    {
    }
}
