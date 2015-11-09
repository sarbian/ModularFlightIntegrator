using System.Linq;
using UnityEngine;

namespace ModularFlightIntegrator
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class MFIManager: MonoBehaviour
    {
        private void Start()
        {
            var fiw = VesselModuleManager.GetWrapper(typeof (FlightIntegrator));
            if (fiw != null && fiw.active)
            {
                print("[MFIManager] FlightIntegrator is active. Deactivating it");

                VesselModuleManager.RemoveModuleOfType(typeof (FlightIntegrator));
            }
            // Should we display this only if we deactivated the stock FI ?
            string msg = "[MFIManager] Current active VesselModule : \n";
            foreach (var vesselModuleWrapper in VesselModuleManager.GetModules(false, false))
            {
                msg += "  " + vesselModuleWrapper.type.ToString() + " active=" + vesselModuleWrapper.active +
                       " order=" + vesselModuleWrapper.order + "\n";
            }
            print(msg);
        }
    }
}
