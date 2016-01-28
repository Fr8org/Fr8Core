using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;
using System.Linq;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Data.Entities
{
    public class PlanDO : RouteNodeDO
    {
        public PlanDO()
        {
            
            //Subroutes = new List<SubrouteDO>();
            /*var startingSubroute = new SubrouteDO();
            startingSubroute.StartingSubroute = true;
            Subroutes.Add(startingSubroute);*/
        }
       
     
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        /*[ForeignKey("StartingSubroute")]
        public int StartingSubrouteId { get; set; }

        public virtual SubrouteDO StartingSubroute { get; set; }*/

        [NotMapped]
        public Guid StartingSubrouteId
        {
            get
            {
                var startingSubroute = ChildNodes.OfType<SubrouteDO>()
                    .SingleOrDefault(pnt => pnt.StartingSubroute == true);
                if (null != startingSubroute)
                {
                    return startingSubroute.Id;
                }
                else
                {
                    return Guid.Empty;
                    //throw new ApplicationException("Starting Subroute doesn't exist.");
                }
            }
        }

        [NotMapped]
        public SubrouteDO StartingSubroute
        {
            get
            {
                return Subroutes.SingleOrDefault(pnt => pnt.StartingSubroute == true);
            }

            set {
                var startingSubroute = Subroutes.SingleOrDefault(pnt => pnt.StartingSubroute == true);
                if (null != startingSubroute)
                    startingSubroute = value;
                else
                {
                    Subroutes.ToList().ForEach(pnt => pnt.StartingSubroute = false);
                    if (value != null) 
                    { 
                        value.StartingSubroute = true;
                        ChildNodes.Add(value);
                    }

                }
            }
        }

        [Required]
        [ForeignKey("RouteStateTemplate")]
        public int RouteState { get; set; }

        public virtual _RouteStateTemplate RouteStateTemplate { get; set; }

        public string Tag { get; set; }
        
        [NotMapped]
        public IEnumerable<SubrouteDO> Subroutes
        {
            get
            {
                return ChildNodes.OfType<SubrouteDO>();
            }
        }

        private static readonly PropertyInfo[] TrackingProperties = 
        {
            typeof(PlanDO).GetProperty("Name"),
            typeof(PlanDO).GetProperty("Tag"),
            typeof(PlanDO).GetProperty("Description"),
        };

        protected override IEnumerable<PropertyInfo> GetTrackingProperties()
        {
            foreach (var trackingProperty in base.GetTrackingProperties())
            {
                yield return trackingProperty;
            }

            foreach (var trackingProperty in TrackingProperties)
            {
                yield return trackingProperty;
            }
        }

        protected override RouteNodeDO CreateNewInstance()
        {
            return new PlanDO();
        }


        public override bool AreContentPropertiesEquals(RouteNodeDO other)
        {
            var plan = (PlanDO)other;
            
            return base.AreContentPropertiesEquals(other) &&
                   Name == plan.Name &&
                   Tag == plan.Tag &&
                   RouteState == plan.RouteState &&
                   Description == plan.Description;
        }

        protected override void CopyProperties(RouteNodeDO source)
        {
            var plan = (PlanDO)source;

            base.CopyProperties(source);
            Name = plan.Name;
            Tag = plan.Tag;
            RouteState = plan.RouteState;
            Description = plan.Description;

        }
        
        private class SmartNavigationalPropertyCollectionProxy<TBase, TDerived> : ICollection<TDerived>
            where TDerived : TBase
        {
            private readonly ICollection<TBase> _baseCollection;

            public int Count
            {
                get
                {
                    return _baseCollection.OfType<TDerived>().Count();
                }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public SmartNavigationalPropertyCollectionProxy(ICollection<TBase> baseCollection)
            {
                _baseCollection = baseCollection;
            }

            public IEnumerator<TDerived> GetEnumerator()
            {
                return _baseCollection.OfType<TDerived>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(TDerived item)
            {
                _baseCollection.Add(item);
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TDerived item)
            {
                return _baseCollection.Contains(item);
            }

            public void CopyTo(TDerived[] array, int arrayIndex)
            {
                foreach (var derived in _baseCollection.OfType<TDerived>())
                {
                    array[arrayIndex] = derived;
                    arrayIndex++;
                }
            }

            public bool Remove(TDerived item)
            {
                return _baseCollection.Remove(item);
            }
        }
    }
}