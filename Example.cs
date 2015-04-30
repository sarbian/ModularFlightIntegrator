using ModularFI;
using UnityEngine;

namespace MyFunMod
{
    // this is a really simple example of how to use ModularFlightIntegrator
    // This one would replace the drag calculation to remove drag whole
    // And make stuff hot
    // Obviously you would have to uncomment the lines for it to work.
    
    /*
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class Example : MonoBehaviour
    {

        public void Awake()
        {
            Debug.Log("Registering our calls in ModularFlightIntegrator");
            ModularFlightIntegrator.RegisterCalculateDragValueOverride(IDontLikeDrag);
            ModularFlightIntegrator.RegisterUpdateConvectionOverride(ProcessUpdateConvection);
        }

        private void ProcessUpdateConvection(ModularFlightIntegrator fi, FlightIntegrator.PartThermalData ptd)
        {
            ptd.part.temperature = 4000;
        }

        private static double IDontLikeDrag(ModularFlightIntegrator fi, Part part)
        {
            return 0;
        }
    }
    */
}
