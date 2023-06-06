using Naz.Abp.DynamicFilters.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Naz.Abp.DynamicFilters.Application
{
    public class DynamicFiltersAppService: ApplicationService, IDynamicFiltersAppService
    {
        public Task<Dictionary<string,string>> GetAll()
        {
            return Task.FromResult(OperationsHelper.GetOperations());
        }
    }
}
