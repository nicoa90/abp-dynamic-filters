using Naz.Abp.DynamicFilters.Application.Contracts;
using Volo.Abp.Modularity;

namespace Naz.Abp.DynamicFilters.Application
{
    [DependsOn(typeof(DynamicFiltersApplicationContractsModule))]
    public class DynamicFiltersApplicationModule: AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
        }
    }
}