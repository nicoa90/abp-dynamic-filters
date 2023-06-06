using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naz.Abp.DynamicFilters.Application.Contracts.Dtos
{
    public class FilterDto
    {
        public FilterDto(string key, string operation, string value)
        {
            Key = key;
            Operation = operation;
            Value = value;
        }

        public string Key { get; private set; }
        public string Operation { get; private set; }
        public string Value { get; private set; }
    }
}
