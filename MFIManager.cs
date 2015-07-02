using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModularFlightIntegrator
{

    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class MFIManager: MonoBehaviour
    {
        private void Start()
        {
            string msg = "MFIManager Start " + HighLogic.LoadedScene + ". Current modules coVesselModule : \n";

            foreach (var vesselModuleWrapper in VesselModuleManager.GetModules(false, false))
            {
                msg += "  " + vesselModuleWrapper.type.ToString() + " active=" + vesselModuleWrapper.active + " order=" + vesselModuleWrapper.order + "\n";
            }
            print(msg);
            
            VesselModuleManager.RemoveModuleOfType(typeof(FlightIntegrator));
            //VesselModuleManager.SetWrapperActive(typeof(ModularFI.ModularFlightIntegrator), false);
            //VesselModuleManager.SetWrapperActive(typeof(FlightIntegrator), false);

            msg = "MFIManager Start Post RemoveModuleOfType. Current modules coVesselModule : \n";
            foreach (var vesselModuleWrapper in VesselModuleManager.GetModules(false, false))
            {
                msg += "  " + vesselModuleWrapper.type.ToString() + " active=" + vesselModuleWrapper.active + " order=" + vesselModuleWrapper.order + "\n";
            }
            print(msg);


            // TODO : clear the event ?
            GameEvents.onVesselLoaded.Add(OnVesselLoad);
        }

        private void OnVesselLoad(Vessel v)
        {
            print("OnVesselLoad");
            VesselModuleManager.RemoveModulesFromVessel(v);
            VesselModuleManager.AddModulesToVessel(v);

            string msg = "OnVesselLoad. Vessel Component : \n";
            foreach (var c in v.gameObject.GetComponents(typeof(Component)))
            {
                msg += "  " + c.GetType() + "\n";
            }
            print(msg);
        }

    }
}
