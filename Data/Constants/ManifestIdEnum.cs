using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Constants
{
	public enum MT : int
	{
		[Display(Name = "Standard Design-Time Fields")]
		StandardDesignTimeFields = 3,

		[Display(Name = "Standard Payload Keys")]
		StandardPayloadKeys = 4,

		[Display(Name = "Standard Payload Data")]
		StandardPayloadData = 5,

		[Display(Name = "Standard Configuration Controls")]
		StandardConfigurationControls = 6,

		[Display(Name = "Standard Event Report")]
		StandardEventReport = 7,

		[Display(Name = "Standard Event Subscription")]
		StandardEventSubscription = 8,

		[Display(Name = "Standard Table Data")]
		StandardTableData = 9,

		[Display(Name = "Standard File Handle")]
		StandardFileHandle = 10,
	}
}
