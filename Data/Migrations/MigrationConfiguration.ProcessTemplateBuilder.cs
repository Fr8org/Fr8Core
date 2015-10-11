using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Newtonsoft.Json;

namespace Data.Migrations
{
    partial class MigrationConfiguration
    {
        private class ProcessTemplateBuilder
        {
            private readonly string _name;
            /**********************************************************************************/
            // Declarations
            /**********************************************************************************/

            private readonly List<CrateDTO> _crates = new List<CrateDTO>();
            private int _ptId;

            /**********************************************************************************/
            // Functions
            /**********************************************************************************/

            public ProcessTemplateBuilder(string name)
            {
                _name = name;
            }

            /**********************************************************************************/

            public ProcessTemplateBuilder AddCrate(CrateDTO crateDto)
            {
                _crates.Add(crateDto);
                return this;
            }

            /**********************************************************************************/

            public void Store(IUnitOfWork uow)
            {
                StoreTemplate(uow);

                var process = uow.ProcessRepository.GetQuery().FirstOrDefault(x => x.Name == _name);

                var add = process == null;
                
                if (add)
                {
                    process = new ProcessDO();
                }

                ConfigureProcess(process);

                if (add)
                {
                    uow.ProcessRepository.Add(process);
                }
            }

            /**********************************************************************************/

            private void StoreTemplate(IUnitOfWork uow)
            {
                var pt = uow.ProcessTemplateRepository.GetQuery().FirstOrDefault(x => x.Name == _name);
                bool add = pt == null;

                if (add)
                {
                    pt = new ProcessTemplateDO();
                }

                pt.Name = _name;
                pt.Description = "Template for testing";
                pt.CreateDate = DateTime.Now;
                pt.LastUpdated = DateTime.Now;
                pt.ProcessTemplateState = ProcessTemplateState.Inactive; // we don't want this process template can be executed ouside of tests

                if (add)
                {
                    uow.ProcessTemplateRepository.Add(pt);
                    uow.SaveChanges();
                }

                _ptId = pt.Id;
            }

            /**********************************************************************************/

            private void ConfigureProcess(ProcessDO process)
            {
                process.Name = _name;
                process.ProcessTemplateId = _ptId;
                process.ProcessState = ProcessState.Executing;

                process.CrateStorage = JsonConvert.SerializeObject(new
                {
                    crates = _crates
                });
            }

            /**********************************************************************************/
        }
    }
}
