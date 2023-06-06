# ABP.IO Dynamic Filters

Application and Contracts modules to add the necesary services so you can add dynamic filters to your GetAll endpoints.


## Installation

You simple need to install in your ApplicationModule the package from [nuget](https://www.nuget.org/packages/Naz.Abp.DynamicFilters.Application) or:

```
dotnet add package Naz.Abp.DynamicFilters.Application
```

Then install in your ApplicationContractsModule the package from [nuget](https://www.nuget.org/packages/Naz.Abp.DynamicFilters.Application.Contracts) or:

```
dotnet add package Naz.Abp.DynamicFilters.Application.Contracts
```

## Usage

### Application Module

Use de defined FilteredResultRequestDto wich is an extension of PagedAndSortedResultRequestDto as the TGetListInput of your ApplicationService:

```cs
public class ProductAppService : CrudAppService<Product, ProductDto, int, FilteredResultRequestDto>
```

This will give you access to Filters and Search parameters.

Then override the CreateFilteredQueryAsync method and build the query as follow:

```cs
protected override async Task<IQueryable<Product>> CreateFilteredQueryAsync(FilteredResultRequestDto input)
{
    return DynamicFiltersBuilder<Product>
            .Using(await base.CreateFilteredQueryAsync(input))
            .WithFilters(input.Filters)
            .Ignore("IgnoredFilter")
            .WithSearch(input.Search,
                        x => x.Name.Contains(input.Search, StringComparison.InvariantCultureIgnoreCase))
            .Build()
            .GetQueryable();
}
```

- DynamicFiltersBuilder implements a builder pattern for creating a Queryable for your entity (Product in this case).
- The using method recieves de Queryable created from the base method.
- WithFilters recieves the list of DynamicFilters to apply.
- Ignore lets you specify a filter key to be ignored.
- WithSearch recieves an expression to use the Search parameter. In this example we are performing a plain text search over Name and Description.
- Build return the DynamicFilters object with the resulting Queryable, the applied filters and the ignored filters.
- The GetQueryable method from DynamicFilters returns the queryable with all the filters and search applied.

### Calling the API

The way to pass the Dynamic Filters through the API is with an Array of Strings with the following format:
*nameOfTheProperty_operation_value* where:
- NameOfTheProperty is the name of the entity's property you want yo filter
- Operation is the name of the condition you want to use to filter
- Value is the value to use as filter.

For example to filter all products with a name starting with "mouse" and a price less than 100 you should pass:

```
.../api/app/product?Filters=name_startsWith_mouse&Filters=price_lt_100
```

These are the operations you can use:

| Operation     | Code          |
| ------------- |:-------------:|
| Equals        | equals        |
| Less than     | lt            |
| Less than or equals | lte     |
| Greater than  | gt            |
| Greater than or equals | gte  |
| Not equals    | notEquals     |
| Starts with   | startsWith    |
| Ends with     | endsWith      |
| Contains      | contains      |
| Not contains  | notContains   |
| Date is       | dateIs        |
| Date before   | dateBefore    |
| Date after    | dateAfter     |
| Date is not   | dateIsNot     |

### Expose operations

If you want to have and endpoint where all the posible operations are exposed you need to:

- Add the module depency on your ApplicationModuleClass like this: 
```cs
...
DependsOn(typeof(DynamicFiltersApplicationModule))
...
public class YourApplicationModule : AbpModule
```

- Add the module to the ConventionalControllers in your HostModule definition 

```cs
private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(YourApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(DynamicFiltersApplicationModule).Assembly);
        });
    }
```

When you run the API you will see a new Api group called DynamicFilters with a Get endpoint.