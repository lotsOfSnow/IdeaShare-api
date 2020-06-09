using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;

namespace IdeaShare.Extensions.Microsoft.AspNetCore.Mvc.ModelBinding
{
    public static class ModelStateDictionaryExtensions
    {
        public static void AddModelErrors(this ModelStateDictionary modelState, IDictionary<string, string> errorsDictionary)
        {
            foreach(var error in errorsDictionary)
            {
                modelState.AddModelError(error.Key, error.Value);
            }
        }
    }
}
