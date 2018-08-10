using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HS
{
    class SendOption
    {
        private string _gateway;

        public string Gateway
        {
            get { return _gateway; }
            set { _gateway = value; }
        }

        private string _devices;

        public string Devices
        {
            get { return _devices; }
            set { _devices = value; }
        }

        private string _order;

        public string Order
        {
            get { return _order; }
            set { _order = value; }
        }

        private string _command;

        public string Command
        {
            get { return _command; }
            set { _command = value; }
        }
        public SendOption(string gateway, string device, string order, string command)
        {
            this.Gateway = gateway;
            this.Devices = device;
            this.Order = order;
            this.Command = command;
        }
    }
}
