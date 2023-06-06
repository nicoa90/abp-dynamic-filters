using Naz.Abp.DynamicFilters.Application.Contracts.Dtos;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace Naz.Abp.DynamicFilters.Application
{
    public class DynamicFiltersBuilder<TEntity>
    {
        private IQueryable<TEntity> _queryable;
        private IEnumerable<FilterDto> _filters = new List<FilterDto>();
        private IEnumerable<FilterDto> _ignoredFilters = new List<FilterDto>();
        private string _delimiter = "_";
        private Func<TEntity, bool> _searchDelegate;

        private DynamicFilters<TEntity> _dynamicFilters;

        private DynamicFiltersBuilder(IQueryable<TEntity> queryable)
        {
            _queryable = queryable;
        }

        public static DynamicFiltersBuilder<TEntity> Using(IQueryable<TEntity> queryable)
        {
            return new DynamicFiltersBuilder<TEntity>(queryable);
        }

        public DynamicFiltersBuilder<TEntity> WithCustomDelimiter(string delimiter)
        {
            _delimiter = delimiter;
            return this;
        }

        public DynamicFiltersBuilder<TEntity> WithFilters(List<string> input)
        {
            if (input == null)
                return this;
            if (input.Any(x => x.Split("_").Count() != 3))
                throw new ArgumentException("Invalid filters", nameof(input));

            _filters = input.Select(x => new FilterDto(
                x.Split(_delimiter)[0],
                x.Split(_delimiter)[1],
                x.Split(_delimiter)[2])
            );
            return this;
        }

        public DynamicFiltersBuilder<TEntity> WithSearch(string searchTerm, Func<TEntity, bool> searchDelegate)
        {
            _searchDelegate = searchDelegate;
            return this;
        }

        public DynamicFiltersBuilder<TEntity> Ignore(params string[] ignoredFilters)
        {
            _ignoredFilters = _filters.Where(x => ignoredFilters.Select(y => y.ToLower()).Any(y => y == x.Key.ToLower()));
            _filters = _filters.Where(x => !_ignoredFilters.Any(y => y.Key == x.Key));
            return this;
        }

        public DynamicFilters<TEntity> Build()
        {
            ApplySearch();
            ApplyFilters();
            _dynamicFilters = new DynamicFilters<TEntity>(_filters.ToList(), _ignoredFilters.ToList(), _queryable);
            return _dynamicFilters;
        }
        private void ApplySearch()
        {
            if (_searchDelegate is null) return;
            _queryable = _queryable.Where(_searchDelegate).AsQueryable();
        }

        private void ApplyFilters()
        {
            if (_filters is null) return;
            foreach (var filter in _filters)
            {
                var property = GetProperties(filter).ToArray();
                _queryable = filter.Operation switch
                {
                    Operations.Equal or Operations.DateIs => _queryable.WhereEqual(filter, property),
                    Operations.LessThan => _queryable.WhereLessThan(filter, property),
                    Operations.LessThanOrEqual or Operations.DateBefore => _queryable.WhereLessThanOrEqual(filter, property),
                    Operations.GreaterThan => _queryable.WhereGreaterThan(filter, property),
                    Operations.GreaterThanOrEqual or Operations.DateAfter => _queryable.WhereGreaterThanOrEqual(filter, property),
                    Operations.NotEqual or Operations.DateIsNot => _queryable.WhereDistinct(filter, property),
                    Operations.StartsWith => _queryable.WhereStartsWith(filter, property),
                    Operations.EndsWith => _queryable.WhereEndsWith(filter, property),
                    Operations.Contains => _queryable.WhereContains(filter, property),
                    Operations.NotContains => _queryable.WhereContains(filter, property),
                    _ => throw new ArgumentException("Invalid", "Operation")
                };
            }
        }

        private static List<PropertyInfo> GetProperties(FilterDto filter)
        {
            List<PropertyInfo> propTree = new();
            var bindings = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
            string[] nameProps = filter.Key.Split(".");
            for (int i = 0; i < nameProps.Length; i++)
            {
                var propertyName = nameProps[i].ToLower();
                PropertyInfo? property = null;
                if (i == 0)
                    property = typeof(TEntity).GetProperty(propertyName, bindings);
                else
                    property = propTree.Last().PropertyType.GetProperty(propertyName, bindings);

                if (property == null)
                    throw new Exception($"Poperty {filter.Key} not found in entity");
                propTree.Add(property);
            }
            return propTree;
        }
    }
}
