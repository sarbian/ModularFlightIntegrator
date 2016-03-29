/*
The MIT License (MIT)

Copyright (c) 2014 sarbian

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModularFI
{
    public class ModularFlightIntegrator : FlightIntegrator
    {

        public delegate void voidDelegate(ModularFlightIntegrator fi);
        public delegate void voidBoolDelegate(ModularFlightIntegrator fi, bool b);
        public delegate double doubleDelegate(ModularFlightIntegrator fi);
        public delegate void voidPartDelegate(ModularFlightIntegrator fi, Part part);
        public delegate double doublePartDelegate(ModularFlightIntegrator fi, Part part);
        public delegate void voidThermalDataDelegate(ModularFlightIntegrator fi, PartThermalData ptd);
        public delegate double doubleThermalDataDelegate(ModularFlightIntegrator fi, PartThermalData ptd);
        public delegate double IntegratePhysicalObjectsDelegate(ModularFlightIntegrator fi, List<GameObject> pObjs, double atmDensity);
        
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

        public List<PartThermalData> PartThermalDataList
        {
            get { return partThermalDataList; }
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
        
        public double FDeltaTime
        {
            get { return fDeltaTime; }
            // set { fDeltaTime = value; } // I feel a set here is a recipe for disaster
        }

        public double FDeltaTimeRecip
        {
            get { return fDeltaTimeRecip; }
            set { fDeltaTimeRecip = value; }
        }

        public double DeltaTime
        {
            get { return deltaTime; }
            set { deltaTime = value; }
        }

        public double FTimeSinceThermo
        {
            get { return fTimeSinceThermo; }
            set { fTimeSinceThermo = value; }
        }

        public double FTimeSinceThermoRecip
        {
            get { return fTimeSinceThermoRecip; }
            set { fTimeSinceThermoRecip = value; }
        }
        
        public bool WasMachConvectionEnabled
        {
            get { return wasMachConvectionEnabled; }
            set { wasMachConvectionEnabled = value; }
        }

        public override void Start()
        {
            print("MFI Start");
            base.Start();

            string msg;
            msg = "Start. VesselModule on vessel : \n";
            foreach (VesselModule vm in vessel.gameObject.GetComponents<VesselModule>())
            {
                msg += "  " + vm.GetType().Name.ToString()+"\n";
            }
            print(msg);
        }

        //protected override void OnDestroy()
        //{
        //    //print("OnDestroy");
        //    base.OnDestroy();
        //}
        //
        //protected override void HookVesselEvents()
        //{
        //    //print("HookVesselEvents");
        //    base.HookVesselEvents();
        //}
        //
        //protected override void UnhookVesselEvents()
        //{
        //    //print("UnhookVesselEvents");
        //    base.UnhookVesselEvents();
        //}
        //

        // TODO : VesselPrecalculate
        //protected override void VesselPrecalculate()
        //{
        //}

        //protected override void FixedUpdate()
        //{
        //    // print("FixedUpdate");
        //
        //    // Update vessel values
        //
        //    // UpdateThermodynamics
        //
        //    // Copy values to part
        //
        //    // UpdateOcclusion
        //
        //    // Integrate Root part
        //
        //    // IntegratePhysicalObjects
        //    base.FixedUpdate();
        //}

        private static doubleDelegate calculateShockTemperatureOverride;

        public static bool RegisterCalculateShockTemperature(doubleDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }

            if (calculateShockTemperatureOverride == null)
            {
                calculateShockTemperatureOverride = dlg;
                return true;
            }

            print("CalculateShockTemperature already has an override");
            return false;
        }

        public override double CalculateShockTemperature()
        {
            if (calculateShockTemperatureOverride == null)
            {
                return base.CalculateShockTemperature();
            }
            else
            {
                return calculateShockTemperatureOverride(this);
            }
        }

        public double BaseFICalculateShockTemperature()
        {
            return base.CalculateShockTemperature();
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

        //TODO : public virtual void ThermalIntegrationPass(bool averageWithPrevious)

        private static doubleDelegate calculateAnalyticTemperatureOverride;

        public static bool RegisterCalculateAnalyticTemperature(doubleDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }

            if (calculateAnalyticTemperatureOverride == null)
            {
                calculateAnalyticTemperatureOverride = dlg;
                return true;
            }

            print("CalculateAnalyticTemperature already has an override");
            return false;
        }

        public override double CalculateAnalyticTemperature()
        {
            if (calculateAnalyticTemperatureOverride == null)
            {
                return base.CalculateAnalyticTemperature();
            }
            else
            {
                return calculateAnalyticTemperatureOverride(this);
            }
        }

        public double BaseFICalculateAnalyticTemperature()
        {
            return base.CalculateAnalyticTemperature();
        }

        private static voidBoolDelegate updateOcclusionOverride;

        public static bool RegisterUpdateOcclusionOverride(voidBoolDelegate dlg)
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

        protected override void UpdateOcclusion(bool all)
        {
            if (updateOcclusionOverride == null)
            {
                base.UpdateOcclusion(all);
            }
            else
            {
                updateOcclusionOverride(this, all);
            }
        }

        public void BaseFIUpdateOcclusion(bool all)
        {
            base.UpdateOcclusion(all);
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
        
        private static voidDelegate calculatePressureOverride;

        public static bool RegisterCalculatePressureOverride(voidDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (calculatePressureOverride == null)
            {
                calculatePressureOverride = dlg;
                return true;
            }

            print("CalculatePressure already has an override");
            return false;
        }

        protected override void CalculatePressure()
        {
            if (calculatePressureOverride == null)
            {
                base.CalculatePressure();
            }
            else
            {
                calculatePressureOverride(this);
            }
        }

        public void BaseFICalculatePressure()
        {
            base.CalculatePressure();
        }
        
        private static voidDelegate calculateSunBodyFluxOverride;
        private static voidDelegate calculateSunBodyFluxPre;
        private static voidDelegate calculateSunBodyFluxPost;

        public static bool RegisterCalculateSunBodyFluxOverride(voidDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (calculateSunBodyFluxOverride == null)
            {
                calculateSunBodyFluxOverride = dlg;
                return true;
            }

            print("CalculatePressure already has an override");
            return false;
        }

        public static void RegisterCalculateSunBodyFluxPre(voidDelegate dlg)
        {
            calculateSunBodyFluxPre += dlg;
        }

        public static void RegisterCalculateSunBodyFluxPost(voidDelegate dlg)
        {
            calculateSunBodyFluxPost += dlg;
        }

        protected override void CalculateSunBodyFlux()
        {
            if (calculateSunBodyFluxOverride != null)
            {
                calculateSunBodyFluxPre(this);
            }

            if (calculateSunBodyFluxOverride == null)
            {
                base.CalculateSunBodyFlux();
            }
            else
            {
                calculateSunBodyFluxOverride(this);
            }

            if (calculateSunBodyFluxPost != null)
            {
                calculateSunBodyFluxPost(this);
            }
        }

        public void BaseFICalculateSunBodyFlux()
        {
            base.CalculateSunBodyFlux();
        }


        // TODO : CalculateDensityThermalLerp

        // TODO : CalculateBackgroundRadiationTemperature

        // TODO : CalculateConstantsVacuum

        // TODO : CalculateConstantsAtmosphere

        // TODO : CalculateShockTemperature
        
        // TODO : CalculateConvectiveCoefficient
        // TODO : CalculateConvectiveCoefficientNewtonian
        // TODO : CalculateConvectiveCoefficientMach


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

        
        private static voidDelegate updateCompoundPartsOverride;

        public static bool RegisterUpdateCompoundParts(voidDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }

            if (updateCompoundPartsOverride == null)
            {
                updateCompoundPartsOverride = dlg;
                return true;
            }

            print("UpdateCompoundParts already has an override");
            return false;
        }

        public override void UpdateCompoundParts()
        {
            if (updateCompoundPartsOverride == null)
            {
                base.UpdateCompoundParts();
            }
            else
            {
                updateCompoundPartsOverride(this);
            }
        }

        public void BaseFIUpdateCompoundParts()
        {
            base.UpdateCompoundParts();
        }
        
        private static voidThermalDataDelegate setSkinPropertiesOverride;

        public static bool RegisterSetSkinProperties(voidThermalDataDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }

            if (setSkinPropertiesOverride == null)
            {
                setSkinPropertiesOverride = dlg;
                return true;
            }

            print("UpdateCompoundParts already has an override");
            return false;
        }

        public override void SetSkinProperties(PartThermalData ptd)
        {
            if (setSkinPropertiesOverride == null)
            {
                base.SetSkinProperties(ptd);
            }
            else
            {
                setSkinPropertiesOverride(this, ptd);
            }
        }

        public void BaseFIetSkinPropertie(PartThermalData ptd)
        {
            base.SetSkinProperties(ptd);
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

        public override void UpdateConduction()
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


        // TODO : public virtual double GetUnifiedSkinTemp

        // TODO : UnifySkinTemp

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

        //protected override void UpdateConvection(PartThermalData ptd)
        //{
        //    if (updateConvectionOverride == null)
        //    {
        //        base.UpdateConvection(ptd);
        //    }
        //    else
        //    {
        //        updateConvectionOverride(this, ptd);
        //    }
        //}
        //
        //public void BaseFIUpdateConvection(PartThermalData ptd)
        //{
        //    base.UpdateConvection(ptd);
        //}

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

        public override void UpdateRadiation(PartThermalData ptd)
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

        // 

        private static doubleThermalDataDelegate updateGetSunAreaOverride;

        public static bool RegisterGetSunAreaOverride(doubleThermalDataDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (updateGetSunAreaOverride == null)
            {
                updateGetSunAreaOverride = dlg;
                return true;
            }

            print("GetSunArea already has an override");
            return false;
        }

        public override double GetSunArea(PartThermalData ptd)
        {
            if (updateGetSunAreaOverride == null)
            {
                return base.GetSunArea(ptd);
            }
            else
            {
                return updateGetSunAreaOverride(this, ptd);
            }
        }

        public double BaseFIGetSunArea(PartThermalData ptd)
        {
            return base.GetSunArea(ptd);
        }

        //

        private static doubleThermalDataDelegate getBodyAreaOverride;

        public static bool RegisterGetBodyAreaOverride(doubleThermalDataDelegate dlg)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }


            if (getBodyAreaOverride == null)
            {
                getBodyAreaOverride = dlg;
                return true;
            }

            print("GetBodyArea already has an override");
            return false;
        }

        public override double GetBodyArea(PartThermalData ptd)
        {
            if (getBodyAreaOverride == null)
            {
                return base.GetBodyArea(ptd);
            }
            else
            {
                return getBodyAreaOverride(this, ptd);
            }
        }

        public double BaseFIBodyArea(PartThermalData ptd)
        {
            return base.GetBodyArea(ptd);
        }
        //





        //protected override double CalculateDragValue_Spherical(Part part)
        //{
        //    return base.CalculateDragValue_Spherical(part);
        //}
        //
        //protected override double CalculateDragValue_Cylindrical(Part part)
        //{
        //    return base.CalculateDragValue_Cylindrical(part);
        //}
        //
        //protected override double CalculateDragValue_Conic(Part part)
        //{
        //    return base.CalculateDragValue_Conic(part);
        //}
        //
        //protected override double CalculateDragValue_Cube(Part part)
        //{
        //    return base.CalculateDragValue_Cube(part);
        //}

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