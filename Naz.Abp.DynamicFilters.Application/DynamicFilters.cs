using Naz.Abp.DynamicFilters.Application.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naz.Abp.DynamicFilters.Application
{
    public class DynamicFilters<TEntity>
    {
        public IEnumerable<FilterDto> Filters { get; private set; }
        public IEnumerable<FilterDto> IgnoredFilters { get; private set; }
        private IQueryable<TEntity> _queryable;

        internal DynamicFilters(IEnumerable<FilterDto> filters, IEnumerable<FilterDto> ignoredFilters,  IQueryable<TEntity> queryable)
        {
            Filters = filters;
            IgnoredFilters = ignoredFilters;
            _queryable = queryable;
        }

        public IQueryable<TEntity> GetQueryable()
        {
            return _queryable;
        }
    }
}
