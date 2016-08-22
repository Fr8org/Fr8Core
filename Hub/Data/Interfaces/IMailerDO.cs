using System;
using System.Collections.Generic;

namespace Data.Interfaces
{
    public interface IMailerDO : IBaseDO
    {
        int Id { get; set; }
        string Handler { get; set; }
        string TemplateName { get; set; }
        IDictionary<String, Object> MergeData { get; }
        IEmailDO Email { get; set; }
        string Footer { get; set; }
    }
}