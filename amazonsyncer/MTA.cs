using System;
using System.Configuration;
using System.Net.Http;
using System.ServiceProcess;
using System.Threading;

namespace MTAService
{
    public partial class MTA : ServiceBase
    {
        #region Constructors

        public MTA()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// This is start method of MTAService
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            ThreadStart threadStart = SyncMTAProduct;
            Thread thread = new Thread(threadStart);
            thread.Start();
        }

        /// <summary>
        /// This method is used to call amazon product ayncer url, to update products
        /// Then method will speep for some defined time and again to same task.
        /// </summary>
        private void SyncMTAProduct()
        {
            string MTAUrl = ConfigurationManager.AppSettings["MTAProductSyncerUrl"];
            while (true)
            {
                HttpClient httpClientMTASyncer = new HttpClient();
                HttpContent httpContentMTASyncer = new StringContent("");
                HttpResponseMessage responseMTASyncer =
                    httpClientMTASyncer.PostAsync(MTAUrl, httpContentMTASyncer).Result;
                Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["TimerTime"]));
            }
        }

        #endregion
    }
}
