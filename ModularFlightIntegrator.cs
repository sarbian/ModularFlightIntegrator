using System.Collections.Generic;
using UnityEngine;

namespace ModularFI
{
    public class ModularFlightIntegrator : FlightIntegrator
    {

        public delegate void voidDelegate(ModularFlightIntegrator fi);
        public delegate void voidPartDelegate(ModularFlightIntegrator fi, Part part);
        public delegate double doublePartDelegate(ModularFlightIntegrator fi, Part part);
        public delegate void voidThermalDataDelegate(ModularFlightIntegrator fi, PartThermalData ptd);
        public delegate double IntegratePhysicalObjectsDelegate(ModularFlightIntegrator fi, List<GameObject> pObjs, double atmDensity);

        //This thing is here because the default FlightIntegrator is still applied to the first vessel loaded.
        //So we need to sort through all the FlightIntegrators on start and remove it.  Fortunately, only having to do it on one makes things a lot
        static bool started = false;

        // Properties to access the FlightIntegrator protected field
        // Some should be readonly I guess

        public Transform IntegratorTransform
        {
            get { return integratorTransform; }
            set { integratorTransform = value; }
        }

        public Part PartRef
        {
            get { return partRef; }
            set { partRef = value; }
        }

        public CelestialBody CurrentMainBody
        {
            get { return currentMainBody; }
            set { currentMainBody = value; }
        }

        public Vessel Vessel
        {
            get { return vessel; }
            set { vessel = value; }
        }

        public PhysicsGlobals.LiftingSurfaceCurve LiftCurves
        {
            get { return liftCurves; }
            set { liftCurves = value; }
        }

        public double DensityThermalLerp
        {
            get { return densityThermalLerp; }
            set { densityThermalLerp = value; }
        }

        public int PartCount
        {
            get { return partCount; }
            set { partCount = value; }
        }

        public static int SunLayerMask
        {
            get { return sunLayerMask; }
            set { sunLayerMask = value; }
        }

        public bool RecreateThermalGraph
        {
            get { return recreateThermalGraph; }
            set { recreateThermalGraph = value; }
        }

        public int PartThermalDataCount
        {
            get { return partThermalDataCount; }
            set { partThermalDataCount = value; }
        }

        public bool IsAnalytical
        {
            get { return isAnalytical; }
            set { isAnalytical = value; }
        }

        public double WarpReciprocal
        {
            get { return warpReciprocal; }
            set { warpReciprocal = value; }
        }

        public bool WasMachConvectionEnabled
        {
            get { return wasMachConvectionEnabled; }
            set { wasMachConvectionEnabled = value; }
        }



        // Awake fire when getting to the Flight Scene, not sooner
        protected void Awake()
        {

            string msg = "Awake. Current modules coVesselModule : \n";
            VesselModuleManager.RemoveModuleOfType(typeof(FlightIntegrator));
            foreach (var vesselModuleWrapper in VesselModuleManager.GetModules(false, false))
            {
                msg += "  " + vesselModuleWrapper.type.ToString() + " active=" + vesselModuleWrapper.active + " order=" + vesselModuleWrapper.order + "\n";
            }
            print(msg);

        }

        protected override void Start()
        {
            base.Start();
            string msg;
            if (!started)
            {
                msg = "Initial start; FlightIntegrator cleanup: \n";
                if (vessel)
                {
                    FlightIntegrator[] integrators = vessel.GetComponents<FlightIntegrator>();
                    for (int i = 0; i < integrators.Length; i++)
                    {
                        FlightIntegrator fi = integrators[i];
                        if (fi == null)
                            continue;
                        msg += "  " + fi.GetType().ToString() + "\n";
                        if (fi != this)
                            GameObject.Destroy(fi);
                    }
                }
                print(msg);
                started = true;
            } 
            msg = "Start. Current modules coVesselModule : \n";
            foreach (var vesselModuleWrapper in VesselModuleManager.GetModules(false, false))
            {
                msg += "  " + vesselModuleWrapper.type.ToString() + " active=" + vesselModuleWrapper.active + " order=" + vesselModuleWrapper.order + "\n";
            }
            print(msg);

        }

        protected override void OnDestroy()
        {
            print("OnDestroy");
            base.OnDestroy();
        }

        protected override void HookVesselEvents()
        {
            print("HookVesselEvents");
            base.HookVesselEvents();
        }

        protected override void UnhookVesselEvents()
        {
            print("UnhookVesselEvents");
            base.UnhookVesselEvents();
        }

        protected override void FixedUpdate()
        {
            // print("FixedUpdate");

            // Update vessel values

            // UpdateThermodynamics

            // Copy values to part

            // UpdateOcclusion

            // Integrate Root part

            // IntegratePhysicalObjects

            base.FixedUpdate();
        }

        private static voidDelegate updateThermodynamicsOverride;
        private static voidDelegate updateThermodynamicsPre;
        private static voidDelegate updateThermodynamicsPost;

        public static bool RegisterUpdateThermodynamicsOverride(voidDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (updateThermodynamicsOverride == null)
            {
                updateThermodynamicsOverride = dlg;
                return true;
            }

            print("UpdateThermodynamics already has an override");
            return false;
        }

        public static void RegisterUpdateThermodynamicsPre(voidDelegate dlg)
        {
            updateThermodynamicsPre += dlg;
        }

        public static void RegisterUpdateThermodynamicsPost(voidDelegate dlg)
        {
            updateThermodynamicsPost += dlg;
        }

        protected override void UpdateThermodynamics()
        {
            // UpdateThermalGraph

            // UpdateConduction

            // UpdateConvection

            // UpdateRadiation

            if (updateThermodynamicsPre != null)
            {
                updateThermodynamicsPre(this);
            }

            if (updateThermodynamicsOverride == null)
            {
                base.UpdateThermodynamics();
            }
            else
            {
                updateThermodynamicsOverride(this);
            }

            if (updateThermodynamicsPost != null)
            {
                updateThermodynamicsPost(this);
            }

        }

        public void BaseFIUpdateThermodynamics()
        {
            base.UpdateThermodynamics();
        }

        private static voidDelegate updateOcclusionOverride;

        public static bool RegisterUpdateOcclusionOverride(voidDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (updateOcclusionOverride == null)
            {
                updateOcclusionOverride = dlg;
                return true;
            }

            print("UpdateOcclusion already has an override");
            return false;
        }

        protected override void UpdateOcclusion()
        {
            if (updateOcclusionOverride == null)
            {
                base.UpdateOcclusion();
            }
            else
            {
                updateOcclusionOverride(this);
            }
        }

        public void BaseFIUpdateOcclusion()
        {
            base.UpdateOcclusion();
        }

        private static voidPartDelegate integrateOverride;

        public static bool RegisterIntegrateOverride(voidPartDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (integrateOverride == null)
            {
                integrateOverride = dlg;
                return true;
            }

            print("Integrate already has an override");
            return false;
        }

        protected override void Integrate(Part part)
        {
            // Aply Gravity / centrifugal / Coriolis Forces)

            // UpdateAerodynamics

            // Integrate child parts

            if (integrateOverride == null)
            {
                base.Integrate(part);
            }
            else
            {
                integrateOverride(this, part);
            }
        }

        public void BaseFIIntegrate(Part part)
        {
            base.Integrate(part);
        }

        private static IntegratePhysicalObjectsDelegate integratePhysicalObjectsOverride;

        public static bool RegisterIntegratePhysicalObjectsOverride(IntegratePhysicalObjectsDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (integratePhysicalObjectsOverride == null)
            {
                integratePhysicalObjectsOverride = dlg;
                return true;
            }

            print("IntegratePhysicalObjects already has an override");
            return false;
        }

        protected override void IntegratePhysicalObjects(List<GameObject> pObjs, double atmDensity)
        {
            if (integratePhysicalObjectsOverride == null)
            {
                base.IntegratePhysicalObjects(pObjs, atmDensity);
            }
            else
            {
                integratePhysicalObjectsOverride(this, pObjs, atmDensity);
            }
        }

        public void BaseFIIntegratePhysicalObjects(List<GameObject> pObjs, double atmDensity)
        {
            base.IntegratePhysicalObjects(pObjs, atmDensity);
        }

        private static voidPartDelegate updateAerodynamicsOverride;

        public static bool RegisterUpdateAerodynamicsOverride(voidPartDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (updateAerodynamicsOverride == null)
            {
                updateAerodynamicsOverride = dlg;
                return true;
            }

            print("UpdateAerodynamics already has an override");
            return false;
        }

        protected override void UpdateAerodynamics(Part part)
        {
            // CalculateDragValue

            // CalculateAerodynamicArea
            // CalculateAreaRadiative
            // CalculateAreaExposed

            if (updateAerodynamicsOverride == null)
            {
                base.UpdateAerodynamics(part);
            }
            else
            {
                updateAerodynamicsOverride(this, part);
            }
        }

        public void BaseFIUpdateAerodynamics(Part part)
        {
            base.UpdateAerodynamics(part);
        }

        private static doublePartDelegate calculateDragValueOverride;

        public static bool RegisterCalculateDragValueOverride(doublePartDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (calculateDragValueOverride == null)
            {
                calculateDragValueOverride = dlg;
                return true;
            }

            print("CalculateDragValue already has an override");
            return false;
        }

        protected override double CalculateDragValue(Part part)
        {
            // CalculateDragValue_Spherical
            // CalculateDragValue_Cylindrical
            // CalculateDragValue_Conic
            // CalculateDragValue_Cube

            if (calculateDragValueOverride == null)
            {
                return base.CalculateDragValue(part);
            }
            else
            {
                return calculateDragValueOverride(this, part);
            }
        }

        public double BaseFICalculateDragValue(Part part)
        {
            return base.CalculateDragValue(part);
        }

        private static voidDelegate updateThermalGraphOverride;

        public static bool RegisterUpdateThermalGraphOverride(voidDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (updateThermalGraphOverride == null)
            {
                updateThermalGraphOverride = dlg;
                return true;
            }

            print("UpdateThermalGraph already has an override");
            return false;
        }

        protected override void UpdateThermalGraph()
        {
            if (updateThermalGraphOverride == null)
            {
                base.UpdateThermalGraph();
            }
            else
            {
                updateThermalGraphOverride(this);
            }
        }

        public void BaseFIUpdateThermalGraph()
        {
            base.UpdateThermalGraph();
        }

        private static voidDelegate updateConductionOverride;

        public static bool RegisterUpdateConductionOverride(voidDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (updateConductionOverride == null)
            {
                updateConductionOverride = dlg;
                return true;
            }

            print("UpdateConduction already has an override");
            return false;
        }

        protected override void UpdateConduction()
        {
            if (updateConductionOverride == null)
            {
                base.UpdateConduction();
            }
            else
            {
                updateConductionOverride(this);
            }
        }

        public void BaseFIUpdateConduction()
        {
            base.UpdateConduction();
        }

        private static voidThermalDataDelegate updateConvectionOverride;

        public static bool RegisterUpdateConvectionOverride(voidThermalDataDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (updateConvectionOverride == null)
            {
                updateConvectionOverride = dlg;
                return true;
            }

            print("UpdateConvection already has an override");
            return false;
        }

        protected override void UpdateConvection(PartThermalData ptd)
        {
            if (updateConvectionOverride == null)
            {
                base.UpdateConvection(ptd);
            }
            else
            {
                updateConvectionOverride(this, ptd);
            }
        }

        public void BaseFIUpdateConvection(PartThermalData ptd)
        {
            base.UpdateConvection(ptd);
        }

        private static voidThermalDataDelegate updateRadiationOverride;

        public static bool RegisterUpdateRadiationOverride(voidThermalDataDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (updateRadiationOverride == null)
            {
                updateRadiationOverride = dlg;
                return true;
            }

            print("UpdateConvection already has an override");
            return false;
        }

        protected override void UpdateRadiation(PartThermalData ptd)
        {
            if (updateRadiationOverride == null)
            {
                base.UpdateRadiation(ptd);
            }
            else
            {
                updateRadiationOverride(this, ptd);
            }
        }

        public void BaseFIUpdateRadiation(PartThermalData ptd)
        {
            base.UpdateRadiation(ptd);
        }

        protected override double CalculateDragValue_Spherical(Part part)
        {
            return base.CalculateDragValue_Spherical(part);
        }

        protected override double CalculateDragValue_Cylindrical(Part part)
        {
            return base.CalculateDragValue_Cylindrical(part);
        }

        protected override double CalculateDragValue_Conic(Part part)
        {
            return base.CalculateDragValue_Conic(part);
        }

        protected override double CalculateDragValue_Cube(Part part)
        {
            return base.CalculateDragValue_Cube(part);
        }

        private static doublePartDelegate calculateAerodynamicAreaOverride;

        public static bool RegisterCalculateAerodynamicAreaOverride(doublePartDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (calculateAerodynamicAreaOverride == null)
            {
                calculateAerodynamicAreaOverride = dlg;
                return true;
            }

            print("CalculateAerodynamicArea already has an override");
            return false;
        }

        protected override double CalculateAerodynamicArea(Part part)
        {
            if (calculateAerodynamicAreaOverride == null)
            {
                return base.CalculateAerodynamicArea(part);
            }
            else
            {
                return calculateAerodynamicAreaOverride(this, part);
            }
        }

        public double BaseFICalculateAerodynamicArea(Part part)
        {
            return base.CalculateAerodynamicArea(part);
        }

        private static doublePartDelegate calculateAreaRadiativeOverride;

        public static bool RegisterCalculateAreaRadiativeOverride(doublePartDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (calculateAreaRadiativeOverride == null)
            {
                calculateAreaRadiativeOverride = dlg;
                return true;
            }

            print("CalculateAreaRadiative already has an override");
            return false;
        }

        protected override double CalculateAreaRadiative(Part part)
        {
            if (calculateAreaRadiativeOverride == null)
            {
                return base.CalculateAreaRadiative(part);
            }
            else
            {
                return calculateAreaRadiativeOverride(this, part);
            }
        }

        public double BaseFICalculateAreaRadiative(Part part)
        {
            return base.CalculateAreaRadiative(part);
        }

        private static doublePartDelegate calculateAreaExposedOverride;

        public static bool RegisterCalculateAreaExposedOverride(doublePartDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (calculateAreaExposedOverride == null)
            {
                calculateAreaExposedOverride = dlg;
                return true;
            }

            print("CalculateAreaExposed already has an override");
            return false;
        }

        protected override double CalculateAreaExposed(Part part)
        {
            if (calculateAreaExposedOverride == null)
            {
                return base.CalculateAreaExposed(part);
            }
            else
            {
                return calculateAreaExposedOverride(this, part);
            }
        }

        public double BaseFICalculateAreaExposed(Part part)
        {
            return base.CalculateAreaExposed(part);
        }

        static void print(string msg)
        {
            MonoBehaviour.print("[ModularFlightIntegrator] " + msg);
        }


    }

}