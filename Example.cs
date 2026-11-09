using ModularFI;
using UnityEngine;

namespace MyFunMod
{
    // This is a really simple example of how to use ModularFlightIntegrator.
    // It removes drag altogether and doubles the convective heating.
    // Obviously you would have to uncomment the lines for it to work.
    //
    // Register once, from the Space Centre (KSPAddon with once = true):
    // each hook has one slot, the first registration wins and the method
    // returns false for the others. Call the matching BaseFI* method
    // from an override to get the stock behaviour.

    /*
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    class Example : MonoBehaviour
    {

        public void Awake()
        {
            Debug.Log("Registering our calls in ModularFlightIntegrator");
            if (!ModularFlightIntegrator.RegisterCalculateDragValueOverride(IDontLikeDrag))
                Debug.Log("Another mod already overrides CalculateDragValue");
            if (!ModularFlightIntegrator.RegisterUpdateConvectionOverride(ProcessUpdateConvection))
                Debug.Log("Another mod already overrides UpdateConvection");
        }

        private static void ProcessUpdateConvection(ModularFlightIntegrator fi, PartThermalData ptd)
        {
            fi.BaseFIUpdateConvection(ptd);
            ptd.convectionFlux *= 2;
        }

        private static double IDontLikeDrag(ModularFlightIntegrator fi, Part part)
        {
            return 0;
        }
    }
    */
}
