using UnityEngine;

namespace ModularFI
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class MFIManager: MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            var fiw = VesselModuleManager.GetWrapper(typeof (FlightIntegrator));
            if (fiw != null && fiw.active)
            {
                print("[MFIManager] FlightIntegrator is active. Deactivating it");

                VesselModuleManager.SetWrapperActive(typeof (FlightIntegrator), false);
            }
            // Should we display this only if we deactivated the stock FI ?
            string msg = "[MFIManager] Current active VesselModule : \n";
            foreach (var vesselModuleWrapper in VesselModuleManager.GetModules(false, false))
            {
                msg += "[MFIManager]  " + vesselModuleWrapper.type.ToString() + " active=" + vesselModuleWrapper.active +
                       " order=" + vesselModuleWrapper.order + "\n";
            }
            print(msg);

            GameEvents.onVesselPrecalcAssign.Add(AddModularPrecalc);
        }

        private void OnDestroy()
        {
            GameEvents.onVesselPrecalcAssign.Remove(AddModularPrecalc);
        }

        private void AddModularPrecalc(Vessel vessel)
        {
            if (!vessel.gameObject.GetComponent<ModularVesselPrecalculate>())
            {
                print("[MFIManager] Adding ModularVesselPrecalculate");
                vessel.gameObject.AddComponent<ModularVesselPrecalculate>();
            }
            print("[MFIManager] listing vessel " + vessel.vesselName + " Components");
            foreach (Component component in vessel.gameObject.GetComponents<Component>())
            {
                print("[MFIManager] " + component.GetType().Name);
            }
        }
    }
}
