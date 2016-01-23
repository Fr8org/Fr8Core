using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;
using System.Linq;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Data.Entities
{
    public class RouteDO : RouteNodeDO
    {
        public RouteDO()
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

        public virtual Fr8AccountDO Fr8Account { get; set; }

        public string Tag { get; set; }

        [InverseProperty("Route")]
        public virtual ICollection<ContainerDO> ChildContainers { get; set; }

        [NotMapped]
        public IEnumerable<SubrouteDO> Subroutes
        {
            get
            {
                return ChildNodes.OfType<SubrouteDO>();
            }
        }

        public override RouteNodeDO Clone()
        {
            return new RouteDO()
            {
                Ordering = this.Ordering,
                Name = this.Name,
                Description = this.Description,
                RouteState = this.RouteState,
                Fr8Account = this.Fr8Account
            };
        }

        public override void BeforeCreate()
        {
            base.BeforeCreate();
            RootRouteNode = this;
        }

        public override void BeforeSave()
        {
            base.BeforeSave();
            RootRouteNode = this;
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