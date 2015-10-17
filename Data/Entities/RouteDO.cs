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
        public int StartingSubrouteId
        {
            get
            {
                var startingSubroute = RouteNodes.OfType<SubrouteDO>().SingleOrDefault(pnt => pnt.StartingSubroute == true);
                if (null != startingSubroute)
                    return startingSubroute.Id;
                else
                {
                    return 0;
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
                    value.StartingSubroute = true;
                    RouteNodes.Add(value);
                }
            }
        }

        [Required]
        [ForeignKey("RouteStateTemplate")]
        public int RouteState { get; set; }

        public virtual _RouteStateTemplate RouteStateTemplate { get; set; }

        public virtual Fr8AccountDO Fr8Account { get; set; }

        [InverseProperty("Route")]
        public virtual ICollection<ContainerDO> ChildContainers { get; set; }

        [NotMapped]
        public IEnumerable<SubrouteDO> Subroutes
        {
            get
            {
                return RouteNodes.OfType<SubrouteDO>();
            }
        }
    }
}