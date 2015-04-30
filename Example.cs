using ModularFI;
using UnityEngine;

namespace MyFunMod
{
    // this is a really simple example of how to use ModularFlightIntegrator
    // This one would replace the drag calculation to remove drag whole
    // Obviously you would have to uncomment the lines for it to work.
    
    /*

    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class Example : MonoBehaviour
    {

        public void Awake()
        {
            ModularFI.ModularFlightIntegrator.RegisterCalculateDragValueOverride(IDontLikeDrag);
        }


        private static double IDontLikeDrag(ModularFlightIntegrator fi, Part part)
        {
            return 0;
        }
    }
    */
}
