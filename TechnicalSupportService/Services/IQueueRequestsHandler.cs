namespace TechnicalSupportService.Services
{
    public interface IQueueRequestsHandler
    {
        void Start();

        int Work { get; }
    }
}
