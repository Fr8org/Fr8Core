using System;

namespace Data.Repositories.MultiTenant
{
    public class MtObject
    {
        public readonly string[] Values;
        public readonly MtTypeDefinition MtTypeDefinition;

        public MtObject(MtTypeDefinition mtTypeDefinition)
        {
            if (mtTypeDefinition.IsComplexType || mtTypeDefinition.IsPrimitive)
            {
                throw new ArgumentException("Can't create MtObject from primitive or complex type definition");
            }

            MtTypeDefinition = mtTypeDefinition;
            Values = new string[mtTypeDefinition.Properties.Count];
        }
    }
}