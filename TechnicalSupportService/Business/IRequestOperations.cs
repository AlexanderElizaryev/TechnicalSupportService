using TechnicalSupportService.Enums;

namespace TechnicalSupportService.Business
{
    public interface IRequestOperations
    {
        bool AddRequest(string id);

        bool RemoveRequest(string id);

        int GetCountRequest();

        RequestStatusType? GetStatusRequest(string id);

        void RunRequest(string employeeID, string requestID);
    }
}
