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
    public class ProcessTemplateDO : ActivityDO
    {
        public ProcessTemplateDO()
        {
            
            //ProcessNodeTemplates = new List<ProcessNodeTemplateDO>();
            /*var startingProcessNodeTemplate = new ProcessNodeTemplateDO();
            startingProcessNodeTemplate.StartingProcessNodeTemplate = true;
            ProcessNodeTemplates.Add(startingProcessNodeTemplate);*/
        }
       
     
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        /*[ForeignKey("StartingProcessNodeTemplate")]
        public int StartingProcessNodeTemplateId { get; set; }

        public virtual ProcessNodeTemplateDO StartingProcessNodeTemplate { get; set; }*/

        [NotMapped]
        public int StartingProcessNodeTemplateId
        {
            get
            {
                var startingProcessNodeTemplate = Activities.OfType<ProcessNodeTemplateDO>().SingleOrDefault(pnt => pnt.StartingProcessNodeTemplate == true);
                if (null != startingProcessNodeTemplate)
                    return startingProcessNodeTemplate.Id;
                else
                {
                    return 0;
                    //throw new ApplicationException("Starting ProcessNodeTemplate doesn't exist.");
                }
            }
        }

        [NotMapped]
        public ProcessNodeTemplateDO StartingProcessNodeTemplate
        {
            get
            {
                return ProcessNodeTemplates.SingleOrDefault(pnt => pnt.StartingProcessNodeTemplate == true);
            }

            set {
                var startingProcessNodeTemplate = ProcessNodeTemplates.SingleOrDefault(pnt => pnt.StartingProcessNodeTemplate == true);
                if (null != startingProcessNodeTemplate)
                    startingProcessNodeTemplate = value;
                else
                {
                    ProcessNodeTemplates.ToList().ForEach(pnt => pnt.StartingProcessNodeTemplate = false);
                    value.StartingProcessNodeTemplate = true;
                    Activities.Add(value);
                }
            }
        }

        [Required]
        [ForeignKey("ProcessTemplateStateTemplate")]
        public int ProcessTemplateState { get; set; }

        public virtual _ProcessTemplateStateTemplate ProcessTemplateStateTemplate { get; set; }

        public virtual DockyardAccountDO DockyardAccount { get; set; }

        [InverseProperty("ProcessTemplate")]
        public virtual ICollection<ContainerDO> ChildContainer { get; set; }

        [NotMapped]
        public IEnumerable<ProcessNodeTemplateDO> ProcessNodeTemplates
        {
            get
            {
                return Activities.OfType<ProcessNodeTemplateDO>();
            }
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