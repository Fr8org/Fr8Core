using System;
using Data.Entities;
using Data.Interfaces;
using log4net.Appender;
using log4net.Core;
using StructureMap;
using Utilities;

namespace KwasantCore
{
    public class KwasantLogAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                var config = ObjectFactory.GetInstance<IConfigRepository>();
                if (!config.Get("LogToDatabase", false))
                    return;

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var logDO = new LogDO
                    {
                        Level = loggingEvent.Level.ToString(),
                        Message = loggingEvent.RenderedMessage,
                        Name = loggingEvent.LoggerName
                    };
                    uow.LogRepository.Add(logDO);
                    uow.SaveChanges();
                }
            }
            catch (Exception)
            {
                //No reason to throw an error on our logging system...
            }
        }
    }
}
