using IdeaShare.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace IdeaShare.Api.Models
{
    public class CommaDelimiterArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var key = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(key);

            if(valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            var val = valueProviderResult.FirstValue;

            if(val == null)
            {
                bindingContext.Model = Array.CreateInstance(bindingContext.ModelType.GetElementType(), 0);
            } 
            else
            {
                var elementType = typeof(string);
                var converter = TypeDescriptor.GetConverter(elementType);
                var values = Array.ConvertAll(val.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries),
                    x => { return converter.ConvertFromString(x != null ? x.Trim() : x); });

                var typedValues = Array.CreateInstance(elementType, values.Length);

                values.CopyTo(typedValues, 0);

                bindingContext.Model = typedValues;
                bindingContext.Result = ModelBindingResult.Success(typedValues);
            }

            return Task.CompletedTask;
        }
    }
}
