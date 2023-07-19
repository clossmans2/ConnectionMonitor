using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionMonitor
{
    public class ConnectionOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public int Timeout { get; set; }
        public int SleepInterval { get; set; }

        public ConnectionOptions()
        {
            Host = "8.8.8.8";
            Port = 53;
            Timeout = 1;
            SleepInterval = 1000;
        }

        public ConnectionOptions(string hostIp, int portNum, int retryInterval, int sleepInterval)
        {
            Host = hostIp;
            Port = portNum;
            Timeout = retryInterval;
            SleepInterval = sleepInterval;
        }
    }
}
