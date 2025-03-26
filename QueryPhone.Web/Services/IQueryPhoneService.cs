using QueryPhone.Web.Models;

namespace QueryPhone.Web.Services
{
    public interface IQueryPhoneService
    {
        IAsyncEnumerable<QueryPhoneResultViewModel> QueryAsync(QueryPhoneConditionViewModel vm);

        IEnumerable<string> GetClientNames();
    }
}