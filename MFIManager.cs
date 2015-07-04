using System.Linq;
using UnityEngine;

namespace ModularFlightIntegrator
{

    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class MFIManager: MonoBehaviour
    {
        private void Start()
        {
            VesselModuleManager.RemoveModuleOfType(typeof(FlightIntegrator));

            string msg = "MFIManager Start Post RemoveModuleOfType. Current modules coVesselModule : \n";
            foreach (var vesselModuleWrapper in VesselModuleManager.GetModules(false, false))
            {
                msg += "  " + vesselModuleWrapper.type.ToString() + " active=" + vesselModuleWrapper.active + " order=" + vesselModuleWrapper.order + "\n";
            }
            print(msg);
          
            GameEvents.onVesselLoaded.Add(OnVesselLoad);
        }


        // Vessel Loading initialize the VesselModule and PartModule in a different order than Vessel Creation
        // This hack allow the MFI to be created after the PartModule
        private void OnVesselLoad(Vessel v)
        {
            ModularFI.ModularFlightIntegrator mfi = v.gameObject.GetComponents<ModularFI.ModularFlightIntegrator>().FirstOrDefault();
            if (mfi != null)
            {
                DestroyImmediate(mfi);
                VesselModuleManager.AddModulesToVessel(v);
            }
        }

    }
}
