using System;
using System.Threading.Tasks;
using SendGrid;

namespace terminalUtilities.SendGrid
{
    class TransportWrapper : ITransport
    {
        private readonly ITransport _transport;
        //private readonly ServiceManager<TransportWrapper> _serviceManager;

        public TransportWrapper(ITransport transport)
        {
            if (transport == null)
                throw new ArgumentNullException(nameof(transport));
            _transport = transport;
            //_serviceManager = new ServiceManager<TransportWrapper>("SendGrid Service", "Email Services");
        }

        public async Task DeliverAsync(ISendGrid message)
        {
            //_serviceManager.LogEvent("Sending an email...");
            try
            {
                await _transport.DeliverAsync(message);
                //_serviceManager.LogSucessful("Email sent.");
            }
            catch (Exception ex)
            {
                //_serviceManager.LogFail(ex, "Failed to send email.");
                throw;
            }
        }

        public void Deliver(ISendGrid message)
        {
            //_serviceManager.LogEvent("Sending an email...");
            try
            {
                _transport.Deliver(message);
                //_serviceManager.LogSucessful("Email sent.");
            }
            catch (Exception ex)
            {
                //_serviceManager.LogFail(ex, "Failed to send email.");
                throw;
            }
        }
    }
}
