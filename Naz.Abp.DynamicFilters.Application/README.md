# ABP.IO Dynamic Filters

Application and Contracts modules with the services you need to add dynamic filters to your GetAll endpoints.

## Installation

You simply need to install the package from [nuget](https://www.nuget.org/packages/Naz.Abp.DynamicFilters.Application) in your ApplicationModule, or:

```
dotnet add package Naz.Abp.DynamicFilters.Application
```

Then in your ApplicationContractsModule, install the package from [nuget](https://www.nuget.org/packages/Naz.Abp.DynamicFilters.Application.Contracts) or:

```
dotnet add package Naz.Abp.DynamicFilters.Application.Contracts
```

## Usage

### Application Module

Use the defined FilteredResultRequestDto, which is an extension of PagedAndSortedResultRequestDto, as TGetListInput of your ApplicationService:

```cs 
public class ProductAppService : CrudAppService
```
This gives you access to Filters and Search parameters.

Then override the CreateFilteredQueryAsync method and create the query as follows:

```cs 
protected override async Task<> CreateFilteredQueryAsync(FilteredResultRequestDto input)
{ 
    return DynamicFiltersBuilder 
    .Using(await base.CreateFilteredQueryAsync(input)) 
    .WithFilters(input.Filters) 
    .Ignore("IgnoredFilter") 
    .WithSearch(input.Search, x => x.Name.Contains(input.Search, StringComparison.InvariantCultureIgnoreCase)) 
    .Build() 
    .GetQueryable();
}
```

- DynamicFiltersBuilder implements a builder pattern to create a Queryable for your entity (in this case Product).
- The using method receives the queryable created by the base method.
- WithFilters receives the list of DynamicFilters to apply.
- With Ignore you can specify a filter key to be ignored.
- WithSearch receives an expression to use the Search parameter. In this example, a simple text search is performed using Name and Description.
- Build returns the DynamicFilters object with the resulting queryable, filters applied, and filters ignored.
- The GetQueryable method of DynamicFilters returns the queryable with all filters applied and the search.

### Calling the API

DynamicFilters is passed via the API using an array of strings with the following format:
*namederproperty_operation_value* where:
- NamederProperty is the name of the property of the entity you want to filter
- Operation is the name of the condition you want to use for filtering
- Value is the value you want to use as a filter.

For example, to filter all products whose name starts with "mouse" and whose price is less than 100, you should pass this value:

```
.../api/app/product?Filters=name_begins-with_mouse&Filters=price_lt_100
```

These are the operations you can use:

| Operation | Code |
| ------------- |:-------------:|
| Equals | equals |
| Less than | lt |
| Less than or equals | lte |
| Greater than | gt |
| Greater than or equals | gte |
| Not equals | notEquals |
| Starts with | startsWith |
| Ends with | endsWith |
| Contains |
| Not contains | notContains |
| Date is | dateIs |
| Date before | dateBefore |
| Date after | dateAfter |
| Date is not | dateIsNot |

### Expose operations

If you want to have an endpoint where all possible operations are available, you have to do this:

- Add the module dependency to your ApplicationModuleClass as follows:
```cs
...
DependsOn(typeof(DynamicFiltersApplicationModule))
...
public class YourApplicationModule : AbpModule
{}
```

- Add the module to the ConventionalControllers in your HostModule definition

```cs 
private void ConfigureConventionalControllers() 
{ 
    Configure(options => { 
        options.ConventionalControllers.Create(typeof(YourApplicationModule).Assembly); 
        options.ConventionalControllers.Create(typeof(DynamicFiltersApplicationModule).Assembly); 
    }); 
}
```

When you run the API, you will see a new api group named DynamicFilters with a Get endpoint.
