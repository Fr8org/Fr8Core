using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;

namespace Utilities
{
    public class ExceptionHandling
    {
        public static void DisplayValidationErrors(DbEntityValidationException e)
        {
            string errorFormat = @"Validation failed for entity [{0}]. Validation errors:" + Environment.NewLine + @"{1}";
            var errorList = new List<String>();
            foreach (var entityValidationError in e.EntityValidationErrors)
            {
                var entityName = entityValidationError.Entry.Entity.GetType().Name;
                var errors = String.Join(Environment.NewLine, entityValidationError.ValidationErrors.Select(a => a.PropertyName + ": " + a.ErrorMessage));
                errorList.Add(String.Format(errorFormat, entityName, errors));
            }
            throw new Exception(String.Join(Environment.NewLine + Environment.NewLine, errorList) + Environment.NewLine, e);
        }
    }
}
