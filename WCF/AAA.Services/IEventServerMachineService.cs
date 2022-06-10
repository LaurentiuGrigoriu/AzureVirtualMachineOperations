using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace AAA.Services
{
    [ServiceContract]
    public interface IEventServerMachineService
    {
        [OperationContract(IsOneWay = true)]
        void SignalMachineStarted(string machineName);
    }
}
