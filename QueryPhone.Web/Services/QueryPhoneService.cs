using QueryPhone.Core.Clients;
using QueryPhone.Core.Models;
using QueryPhone.Web.Models;
using System.Text.RegularExpressions;

namespace QueryPhone.Web.Services
{
    public class QueryPhoneService : IQueryPhoneService
    {
        private readonly ILogger<QueryPhoneService> _logger;
        private readonly IEnumerable<IQueryPhoneClient> _queryPhoneClients;

        public QueryPhoneService(ILogger<QueryPhoneService> logger, IEnumerable<IQueryPhoneClient> queryPhoneClients)
        {
            _logger = logger;
            _queryPhoneClients = queryPhoneClients;
        }

        public async IAsyncEnumerable<QueryPhoneResultViewModel> QueryAsync(QueryPhoneConditionViewModel vm)
        {
            string phone = GetQueryPhone(vm.Phone);
            if (string.IsNullOrWhiteSpace(phone))
                yield break;

            var checkedClients = _queryPhoneClients.Join(vm.ClientNames,
                ins => ins.Name,
                name => name,
                (ins, name) => ins);

            var tasks = checkedClients.Select(async client => 
            {
                var result = await client.QueryAsync(phone);
                return ConvertViewModel(client.Name, result);
            }).ToList();

            while (tasks.Count > 0)
            {
                Task<QueryPhoneResultViewModel> completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
                yield return await completedTask;
            }
        }

        private static string GetQueryPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return phone;
            return Regex.Replace(phone, @"\D", string.Empty);
        }

        private static QueryPhoneResultViewModel ConvertViewModel(string name, QueryPhoneResult vm)
        { 
            return new QueryPhoneResultViewModel
            {
                Name = name,
                QueryUrl = vm.QueryUrl,
                Success = vm.Success,
                ReportMsgs = vm.ReportMsgs,
                SummaryMsgs = vm.SummaryMsgs
            };
        }
    }
}
