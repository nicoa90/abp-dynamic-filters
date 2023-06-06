using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Naz.Abp.DynamicFilters.Application
{
    public static class Operations
    {
        public const string Equal = "equals";
        public const string LessThan = "lt";
        public const string LessThanOrEqual = "lte";
        public const string GreaterThan = "gt";
        public const string GreaterThanOrEqual = "gte";
        public const string NotEqual = "notEquals";
        public const string StartsWith = "startsWith";
        public const string EndsWith = "endsWith";
        public const string Contains = "contains";
        public const string NotContains = "notContains";
        public const string DateIs = "dateIs";
        public const string DateBefore = "dateBefore";
        public const string DateAfter = "dateAfter";
        public const string DateIsNot = "dateIsNot";
    }

    public static class OperationsHelper
    {
        public static Dictionary<string, string> GetOperations()
        {
            var result = new Dictionary<string, string>();
            FieldInfo[] fields = typeof(Operations).GetFields(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static);
            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly)
                {
                    string name = field.Name;
                    string value = field.GetValue(null).ToString();
                    result.Add(name, value);
                }
            }
            return result;
        }

    }
}
