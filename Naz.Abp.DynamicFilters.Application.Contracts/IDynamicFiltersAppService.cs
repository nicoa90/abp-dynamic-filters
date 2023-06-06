using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Naz.Abp.DynamicFilters.Application.Contracts
{
    public interface IDynamicFiltersAppService: IApplicationService
    {
        Task<Dictionary<string, string>> GetAll();
    }
}
