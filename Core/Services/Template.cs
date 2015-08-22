using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using Utilities;
using Newtonsoft.Json.Linq;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Data.Wrappers;

namespace Core.Services
{
    public class DocuSignxTemplate 
    {
        private readonly IEnvelope _envelope;

        public DocuSignxTemplate()
        {
            //_envelope = ObjectFactory.GetInstance<IEnvelope>();


        }

    }
}

