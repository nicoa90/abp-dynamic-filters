using Microsoft.VisualBasic;
using Naz.Abp.DynamicFilters.Application.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Naz.Abp.DynamicFilters.Application
{
    public static class QueryableExtensions
    {
        internal static IQueryable<T> WhereEqual<T>(this IQueryable<T> query, FilterDto filter, params PropertyInfo[] propertyTree)
        {
            var predicate = GetPredicate<T>(filter, propertyTree, (m, e) => {
                return Expression.Equal(m, e);
            });
            return query.Where(predicate);
        }

        internal static IQueryable<T> WhereLessThan<T>(this IQueryable<T> query, FilterDto filter, params PropertyInfo[] propertyTree)
        {
            var predicate = GetPredicate<T>(filter, propertyTree, (m, e) => {
                return Expression.LessThan(m, e);
            });
            return query.Where(predicate);
        }

        internal static IQueryable<T> WhereLessThanOrEqual<T>(this IQueryable<T> query, FilterDto filter, params PropertyInfo[] propertyTree)
        {
            var predicate = GetPredicate<T>(filter, propertyTree, (m,e) => { 
                return Expression.LessThanOrEqual(m, e); 
            });
            return query.Where(predicate);
        }

        internal static IQueryable<T> WhereGreaterThan<T>(this IQueryable<T> query, FilterDto filter, params PropertyInfo[] propertyTree)
        {
            var predicate = GetPredicate<T>(filter, propertyTree, (m, e) => {
                return Expression.GreaterThan(m, e);
            });
            return query.Where(predicate);
        }

        internal static IQueryable<T> WhereGreaterThanOrEqual<T>(this IQueryable<T> query, FilterDto filter, params PropertyInfo[] propertyTree)
        {
            var predicate = GetPredicate<T>(filter, propertyTree, (m, e) => {
                return Expression.GreaterThanOrEqual(m, e);
            });
            return query.Where(predicate);
        }

        internal static IQueryable<T> WhereDistinct<T>(this IQueryable<T> query, FilterDto filter, params PropertyInfo[] propertyTree)
        {
            var predicate = GetPredicate<T>(filter, propertyTree, (m, e) => {
                return Expression.NotEqual(m, e);
            });
            return query.Where(predicate);
        }

        internal static IQueryable<T> WhereDate<T>(this IQueryable<T> query, FilterDto filter, params PropertyInfo[] propertyTree)
        {
            var filterFrom = new FilterDto(filter.Key, filter.Operation, FromBeginningOfDay(filter.Value));
            var filterTo = new FilterDto(filter.Key, filter.Operation, ToEndOfDay(filter.Value));
            return query.WhereGreaterThanOrEqual(filterFrom, propertyTree)
                .WhereLessThanOrEqual(filterTo, propertyTree);
        }

        internal static IQueryable<T> WhereDatetime<T>(this IQueryable<T> query, FilterDto filter, params PropertyInfo[] propertyTree)
        {
            var filterFrom = new FilterDto(filter.Key, filter.Operation, AddMinutes(filter.Value, -1));
            var filterTo = new FilterDto(filter.Key, filter.Operation, AddMinutes(filter.Value, 1));
            return query.WhereGreaterThanOrEqual(filterFrom, propertyTree)
                .WhereLessThanOrEqual(filterTo, propertyTree);

        }

        internal static IQueryable<T> WhereContains<T>(this IQueryable<T> query, FilterDto filter, params PropertyInfo[] propertyTree)
        {
            var predicate = GetPredicateForStringComparer<T>(filter, propertyTree, DynamicFiltersConsts.ContainsCompareMethod);
            return query.Where(predicate);
        }

        internal static IQueryable<T> WhereStartsWith<T>(this IQueryable<T> query, FilterDto filter, params PropertyInfo[] propertyTree)
        {
            var predicate = GetPredicateForStringComparer<T>(filter, propertyTree, DynamicFiltersConsts.StartsWithCompareMethod);
            return query.Where(predicate);
        }

        internal static IQueryable<T> WhereEndsWith<T>(this IQueryable<T> query, FilterDto filter, params PropertyInfo[] propertyTree)
        {
            var predicate = GetPredicateForStringComparer<T>(filter, propertyTree, DynamicFiltersConsts.EndsWithCompareMethod);
            return query.Where(predicate);
        }

        internal static IQueryable<T> WhereIn<T>(this IQueryable<T> query, FilterDto filter, params PropertyInfo[] propertyTree)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));
            MemberExpression member = CreateMemberExpression(parameter, propertyTree);

            Type propertyType = propertyTree.Last().PropertyType;
            Type genericConstructedList = typeof(List<>).MakeGenericType(propertyType);
            IEnumerable<Expression> constants = filter.Value.Split(',').Select(value => BuildExpression(new FilterDto("", "", value), propertyTree));
            MethodInfo containsMethod = genericConstructedList.GetMethod(DynamicFiltersConsts.ContainsCompareMethod, new Type[] { propertyType });
            ListInitExpression list = Expression.ListInit(Expression.New(genericConstructedList), constants);
            MethodCallExpression call = Expression.Call(list, containsMethod, member);
            Expression<Func<T, bool>> predicate = Expression.Lambda<Func<T, bool>>(call, parameter);
            return query.Where(predicate);
        }
        private static Expression<Func<T, bool>> GetPredicate<T>(FilterDto filter, PropertyInfo[] propertyTree, Func<MemberExpression, Expression, Expression> action)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));
            MemberExpression member = CreateMemberExpression(parameter, propertyTree);
            Expression expression = BuildExpression(filter, propertyTree);

            Expression<Func<T, bool>> predicate = Expression.Lambda<Func<T, bool>>(
                action(member, expression),
                parameter);

            return predicate;
        }

        private static Expression<Func<T, bool>> GetPredicateForStringComparer<T>(FilterDto filter, PropertyInfo[] propertyTree, string compareMode)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));
            MemberExpression member = CreateMemberExpression(parameter, propertyTree);

            MethodInfo compareMethod = typeof(string).GetMethod(compareMode, new Type[] { typeof(string) });

            Expression toLower = Expression.Call(member, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
            Expression expression = Expression.Constant(filter.Value.ToLower(), typeof(string));
            MethodCallExpression call = Expression.Call(toLower, compareMethod, expression);

            Expression<Func<T, bool>> predicate = Expression.Lambda<Func<T, bool>>(call, parameter);

            return predicate;
        }

        private static MemberExpression CreateMemberExpression(ParameterExpression parameter, params PropertyInfo[] propertyTree)
        {
            MemberExpression? member = null;
            for (int i = 0; i < propertyTree.Length; i++)
            {
                if (i == 0)
                    member = Expression.Property(parameter, propertyTree[i].Name);
                else
                    member = Expression.Property(member, propertyTree[i].Name);
            }
            if (member == null)
                throw new Exception($"Member not found for {string.Join('.', propertyTree.Select(x => x.Name))}");
            return member;
        }

        private static Expression BuildExpression(FilterDto filter, params PropertyInfo[] propertyTree)
        {
            var type = propertyTree.Last().PropertyType;

            if(type.IsEnum)
                return Expression.Constant(Enum.ToObject(type, int.Parse(filter.Value)));
            else if(type.IsNullableType() && string.IsNullOrWhiteSpace(filter.Value))
                return Expression.Constant(null);
            else if(type.IsNullableType())
                return ConstantNullable(filter.Value, type);
            else
                return Expression.Constant(Convert.ChangeType(filter.Value, type));
        }

        private static bool IsNullableType(this Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }

        private static Expression ConstantNullable(string value, Type type)
        {
            var converted = Convert.ChangeType(value, type.GetGenericArguments()[0], CultureInfo.InvariantCulture);
            if (type == typeof(DateTime?))
                converted = TimeZoneInfo.ConvertTimeToUtc((DateTime)converted);
            return Expression.Convert(Expression.Constant(converted), type);
        }

        private static string FromBeginningOfDay(string value)
        {
            return DateTime.ParseExact(value, DynamicFiltersConsts.DateFormat, CultureInfo.InvariantCulture)
                .Date.ToString(DynamicFiltersConsts.DateFormat);
        }

        private static string ToEndOfDay(string value)
        {
            return DateTime.ParseExact(value, DynamicFiltersConsts.DateFormat, CultureInfo.InvariantCulture)
                .AddDays(1).AddMinutes(-1).ToString(DynamicFiltersConsts.DateFormat);
        }

        private static string AddMinutes(string value, int minutes)
        {
            return DateTime.ParseExact(value, DynamicFiltersConsts.DateFormat, CultureInfo.InvariantCulture)
                .AddMinutes(minutes).ToString(DynamicFiltersConsts.DateFormat);
        }
    }

}
