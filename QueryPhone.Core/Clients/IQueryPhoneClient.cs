using QueryPhone.Core.Models;

namespace QueryPhone.Core.Clients
{
    public interface IQueryPhoneClient
    {
        string Name { get; }

        Task<QueryPhoneResult> QueryAsync(string phone);
    }
}