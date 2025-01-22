using Microsoft.AspNetCore.Mvc;
using QueryPhone.Web.Models;
using QueryPhone.Web.Services;

namespace QueryPhone.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IQueryPhoneService _queryPhoneService;


        public HomeController(IQueryPhoneService queryPhoneService)
        {
            _queryPhoneService = queryPhoneService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async IAsyncEnumerable<QueryPhoneResultViewModel> QueryPhone([FromBody]QueryPhoneConditionViewModel vm)
        {
            await foreach (var result in _queryPhoneService.QueryAsync(vm))
                yield return result;
        }

    }
}
