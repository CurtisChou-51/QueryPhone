using QueryPhone.Model;

namespace QueryPhone.Clients
{
    public interface IQueryPhoneClient
    {
        string GetName();

        Task<QueryPhoneResult> QueryAsync(string phone);
    }
}