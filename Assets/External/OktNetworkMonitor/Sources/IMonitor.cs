using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Oktagon.Network
{
    public interface IMonitor
    {
        void SetupMonitor(OktNetworkMonitor pMonitor);
        void Destroy();

        bool Record
        {
            get;
            set;
        }

    }

}