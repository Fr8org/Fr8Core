using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace KwasantWeb.App_Start
{
    public class ListValueResolver<TSourceItem, TDestinationItem> : IValueResolver
        where TSourceItem : class
        where TDestinationItem : class
    {
        private readonly Func<TSourceItem, TDestinationItem, bool> _equalityFunc;

        public ListValueResolver(Func<TSourceItem, TDestinationItem, bool> equalityFunc)
        {
            _equalityFunc = equalityFunc;
        }

        public ResolutionResult Resolve(ResolutionResult source)
        {
            var sourceList = (ICollection<TSourceItem>)source.Value;
            var destinationObject = source.Context.DestinationValue;
            var memberInfo = destinationObject.GetType().GetProperty(source.Context.MemberName);
            var destinationList = (IList<TDestinationItem>)memberInfo.GetValue(destinationObject);
            if (sourceList == null)
            {
                return source.New(null);
            }
            if (destinationList == null)
            {
                return source.New(Mapper.Map<IList<TDestinationItem>>(sourceList));
            }

            var itemsToRemove = destinationList
                .Where(destinationItem => !sourceList.Any(sourceItem => _equalityFunc(sourceItem, destinationItem)))
                .ToArray();
            foreach (var destinationItem in itemsToRemove)
            {
                destinationList.Remove(destinationItem);
            }

            foreach (var sourceItem in sourceList)
            {
                var destinationItem = destinationList.FirstOrDefault(d => _equalityFunc(sourceItem, d));
                if (destinationItem != null)
                {
                    Mapper.Map(sourceItem, destinationItem);
                }
                else
                {
                    destinationItem = Mapper.Map<TDestinationItem>(sourceItem);
                    destinationList.Add(destinationItem);
                }
            }
            return source.New(destinationList);
        }
    }
}