using System;
using System.Collections.Generic;
using FluentValidation.Validators;

namespace Data.Validations
{
    


    //=================================================================
    //Custom Validators
    //
    //   ListMustContainAtLeastOneItem
    //   AtLeast   works on strings, which is not true of the built-in "GreaterThan"



    public class ListMustContainAtLeastOneItem<T> : PropertyValidator
    {
        public ListMustContainAtLeastOneItem()
            : base("Property {PropertyName} must contain at least 1 item!")
        {

        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var list = context.PropertyValue as IList<T>;

            if (list == null || list.Count < 1)
            {
                return false;
            }

            return true;
        }
    }

    public class StringMinLength : PropertyValidator
    {
        public int Min { get; private set; }

        public StringMinLength(int min)
            : base("String {PropertyName} must be at least {Min} characters long.")
        {
            Min = min;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            string curString = context.PropertyValue as String;

            if (curString==null || curString.Length<= Min)
            {
                return false;
            }

            return true;
        }
    }


}
