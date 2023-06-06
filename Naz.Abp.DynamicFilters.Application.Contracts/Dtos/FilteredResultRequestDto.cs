using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Naz.Abp.DynamicFilters.Application.Contracts.Dtos
{
    public class FilteredResultRequestDto: PagedAndSortedResultRequestDto
    {
        public List<string> Filters { get; set; }
        public string Search { get; set; } = string.Empty;
    }
}
