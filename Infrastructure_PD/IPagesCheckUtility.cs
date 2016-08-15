using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HubWeb.Templates;

namespace HubWeb.Infrastructure_PD
{
    /// <summary>
    /// Interface for utility which is checking static generated pages
    /// </summary>
    public interface IPagesCheckUtility
    {
        /// <summary>
        /// Check whether static pages exist for all plan templates in directory search index, create that ones which are not exist, and deletes that ones which are not in search index.
        /// </summary>
        /// <returns>Number of generated pages</returns>
        Task<long> CheckPlanTempletesPages();

    }
}
