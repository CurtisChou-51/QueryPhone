using QueryPhone.Model;

namespace QueryPhone.Clients
{
    public interface IQueryPhoneClient
    {
        string Name { get; }

        Task<QueryPhoneResult> QueryAsync(string phone);
    }
}