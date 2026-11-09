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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModularFI
{
    // 9 is the Timing of the stock FlightIntegrator but using it leads to race/exec order problems with FAR+Principia.
    [DefaultExecutionOrder(10)]
    public class ModularFlightIntegrator : FlightIntegrator
    {

        public delegate void voidDelegate(ModularFlightIntegrator fi);
        public delegate void voidBoolDelegate(ModularFlightIntegrator fi, bool b);
        public delegate double doubleDelegate(ModularFlightIntegrator fi);
        public delegate double doubleDoubleDelegate(ModularFlightIntegrator fi, double d);
        public delegate void voidPartDelegate(ModularFlightIntegrator fi, Part part);
        public delegate double doublePartDelegate(ModularFlightIntegrator fi, Part part);
        public delegate void voidThermalDataDelegate(ModularFlightIntegrator fi, PartThermalData ptd);
        public delegate double doubleThermalDataDelegate(ModularFlightIntegrator fi, PartThermalData ptd);
        /// <summary>
        /// Override of IntegratePhysicalObjects, which is void: the return value is ignored.
        /// The delegate keeps its shape so the mods compiled against it keep working.
        /// </summary>
        public delegate double IntegratePhysicalObjectsDelegate(ModularFlightIntegrator fi, List<physicalObject> pObjs, double atmDensity);

        // Properties giving an override access to the FlightIntegrator fields that are
        // protected in stock. The ones with a setter are written by the stock FixedUpdate
        // every step, so a value set from a hook lasts until the next step.

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

        /*public Vessel Vessel
        {
            get { return vessel; }
            set { vessel = value; }
        }*/
        
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

        /// <summary>
        /// Same as the stock partThermalDataList field, which is public since KSP 1.1;
        /// kept for the mods that use this property.
        /// </summary>
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
        
        /// <summary>
        /// The physics step, set by the stock FixedUpdate. Read only: the integrator
        /// derives FDeltaTimeRecip from it.
        /// </summary>
        public double FDeltaTime
        {
            get { return fDeltaTime; }
        }

        /// <summary>
        /// 1 / FDeltaTime. The setter exists for compatibility; a value set here is
        /// overwritten by the stock FixedUpdate on the next step.
        /// </summary>
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

        protected override void OnStart()
        {
            print("MFI Start");
            base.OnStart();

            string msg = "Start. VesselModule on vessel : \n";
            foreach (VesselModule vm in vessel.gameObject.GetComponents<VesselModule>())
            {
                msg += "  " + vm.GetType().Name + "\n";
            }
            // Register our replacement FixedUpdate to run at the same timing as the stock FlightIntegrator
            //TimingManager.UpdateAdd(TimingManager.TimingStage.FlightIntegrator, TimedUpdate);
            //TimingManager.FixedUpdateAdd(TimingManager.TimingStage.FlightIntegrator, TimedFixedUpdate);
            print(msg);
        }

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
        private static voidDelegate fixedUpdateOverride;

        public static bool RegisterFixedUpdateOverride(voidDelegate dlg)
        {
            return HookRegistry.Register(ref fixedUpdateOverride, dlg, "FixedUpdate");
        }

        public static bool UnregisterFixedUpdateOverride(voidDelegate dlg)
        {
            return HookRegistry.Unregister(ref fixedUpdateOverride, dlg, "FixedUpdate");
        }

        // We want the FixedUpdate code to run exactly when we want (after vessel and partmodule).
        // The stock game uses a Unity settings to force the execution order
        // Mods can not use that but can register with TimingManager.FixedUpdateAdd, called in OnStart here.
        // replace our FixedUpdate with something that does nothing since we will call TimedFixedUpdate
        protected override void FixedUpdate()
        {
            if (fixedUpdateOverride == null)
            {
                base.FixedUpdate();
            }
            else
            {
                fixedUpdateOverride(this);
            }
        }
        
        private static doubleDelegate calculateShockTemperatureOverride;

        public static bool RegisterCalculateShockTemperatureOverride(doubleDelegate dlg)
        {
            return HookRegistry.Register(ref calculateShockTemperatureOverride, dlg, "CalculateShockTemperature");
        }

        public static bool UnregisterCalculateShockTemperatureOverride(doubleDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateShockTemperatureOverride, dlg, "CalculateShockTemperature");
        }

        [Obsolete("Use RegisterCalculateShockTemperatureOverride.")]
        public static bool RegisterCalculateShockTemperature(doubleDelegate dlg)
        {
            return RegisterCalculateShockTemperatureOverride(dlg);
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
        
        private static readonly voidDelegate[] NoHandlers = new voidDelegate[0];

        // The Pre/Post lists are kept both as a multicast delegate (what Add/Remove work on)
        // and as an array (what the integrator iterates), so that each handler runs in its
        // own try/catch without allocating an invocation list every step.
        private static voidDelegate[] HandlersOf(voidDelegate list)
        {
            if (list == null)
            {
                return NoHandlers;
            }

            Delegate[] all = list.GetInvocationList();
            voidDelegate[] handlers = new voidDelegate[all.Length];
            for (int i = 0; i < all.Length; i++)
            {
                handlers[i] = (voidDelegate)all[i];
            }

            return handlers;
        }

        // One handler that throws must not stop the others, nor the rest of this FixedUpdate:
        // the forces the part modules queued would be neither applied nor cleared that step.
        private void InvokeHandlers(voidDelegate[] handlers, string hook)
        {
            for (int i = 0; i < handlers.Length; i++)
            {
                try
                {
                    handlers[i](this);
                }
                catch (Exception e)
                {
                    HookRegistry.ReportHandlerException(hook, handlers[i], e);
                }
            }
        }

        private static voidDelegate updateThermodynamicsOverride;
        private static voidDelegate updateThermodynamicsPre;
        private static voidDelegate updateThermodynamicsPost;
        private static voidDelegate[] updateThermodynamicsPreHandlers = NoHandlers;
        private static voidDelegate[] updateThermodynamicsPostHandlers = NoHandlers;

        public static bool RegisterUpdateThermodynamicsOverride(voidDelegate dlg)
        {
            return HookRegistry.Register(ref updateThermodynamicsOverride, dlg, "UpdateThermodynamics");
        }

        public static bool UnregisterUpdateThermodynamicsOverride(voidDelegate dlg)
        {
            return HookRegistry.Unregister(ref updateThermodynamicsOverride, dlg, "UpdateThermodynamics");
        }

        /// <summary>
        /// Adds a handler that runs at the start of every FixedUpdate of the integrator, before the
        /// thermodynamics. "Every FixedUpdate": the stock UpdateThermodynamics only integrates when
        /// fTimeSinceThermo exceeds PhysicsGlobals.ThermalIntegrationMinStep, but this list runs on
        /// every physics step whether or not a thermal step follows, which is what FAR uses it for
        /// (per-part dynamic pressure and angular drag). Handlers run in registration order, each in
        /// its own try/catch, and a handler is only added once.
        /// </summary>
        public static void RegisterUpdateThermodynamicsPre(voidDelegate dlg)
        {
            HookRegistry.Add(ref updateThermodynamicsPre, dlg);
            updateThermodynamicsPreHandlers = HandlersOf(updateThermodynamicsPre);
        }

        public static void UnregisterUpdateThermodynamicsPre(voidDelegate dlg)
        {
            HookRegistry.Remove(ref updateThermodynamicsPre, dlg);
            updateThermodynamicsPreHandlers = HandlersOf(updateThermodynamicsPre);
        }

        /// <summary>
        /// Adds a handler that runs after the thermodynamics on every FixedUpdate of the integrator,
        /// including the steps where no thermal integration happened (see RegisterUpdateThermodynamicsPre).
        /// Handlers run in registration order, each in its own try/catch, and a handler is only added once.
        /// </summary>
        public static void RegisterUpdateThermodynamicsPost(voidDelegate dlg)
        {
            HookRegistry.Add(ref updateThermodynamicsPost, dlg);
            updateThermodynamicsPostHandlers = HandlersOf(updateThermodynamicsPost);
        }

        public static void UnregisterUpdateThermodynamicsPost(voidDelegate dlg)
        {
            HookRegistry.Remove(ref updateThermodynamicsPost, dlg);
            updateThermodynamicsPostHandlers = HandlersOf(updateThermodynamicsPost);
        }

        protected override void UpdateThermodynamics()
        {
            // UpdateThermalGraph

            // UpdateConduction

            // UpdateConvection

            // UpdateRadiation

            InvokeHandlers(updateThermodynamicsPreHandlers, "UpdateThermodynamicsPre");

            if (updateThermodynamicsOverride == null)
            {
                base.UpdateThermodynamics();
            }
            else
            {
                updateThermodynamicsOverride(this);
            }

            InvokeHandlers(updateThermodynamicsPostHandlers, "UpdateThermodynamicsPost");
        }

        public void BaseFIUpdateThermodynamics()
        {
            base.UpdateThermodynamics();
        }

        //TODO : public virtual void ThermalIntegrationPass(bool averageWithPrevious)

        private static doubleDelegate calculateAnalyticTemperatureOverride;

        public static bool RegisterCalculateAnalyticTemperatureOverride(doubleDelegate dlg)
        {
            return HookRegistry.Register(ref calculateAnalyticTemperatureOverride, dlg, "CalculateAnalyticTemperature");
        }

        public static bool UnregisterCalculateAnalyticTemperatureOverride(doubleDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateAnalyticTemperatureOverride, dlg, "CalculateAnalyticTemperature");
        }

        [Obsolete("Use RegisterCalculateAnalyticTemperatureOverride.")]
        public static bool RegisterCalculateAnalyticTemperature(doubleDelegate dlg)
        {
            return RegisterCalculateAnalyticTemperatureOverride(dlg);
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
            return HookRegistry.Register(ref updateOcclusionOverride, dlg, "UpdateOcclusion");
        }

        public static bool UnregisterUpdateOcclusionOverride(voidBoolDelegate dlg)
        {
            return HookRegistry.Unregister(ref updateOcclusionOverride, dlg, "UpdateOcclusion");
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
            return HookRegistry.Register(ref integrateOverride, dlg, "Integrate");
        }

        public static bool UnregisterIntegrateOverride(voidPartDelegate dlg)
        {
            return HookRegistry.Unregister(ref integrateOverride, dlg, "Integrate");
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
            return HookRegistry.Register(ref integratePhysicalObjectsOverride, dlg, "IntegratePhysicalObjects");
        }

        public static bool UnregisterIntegratePhysicalObjectsOverride(IntegratePhysicalObjectsDelegate dlg)
        {
            return HookRegistry.Unregister(ref integratePhysicalObjectsOverride, dlg, "IntegratePhysicalObjects");
        }

        protected override void IntegratePhysicalObjects(List<physicalObject> pObjs, double atmDensity)
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

        public void BaseFIIntegratePhysicalObjects(List<physicalObject> pObjs, double atmDensity)
        {
            base.IntegratePhysicalObjects(pObjs, atmDensity);
        }

        private static voidDelegate calculatePressureOverride;

        public static bool RegisterCalculatePressureOverride(voidDelegate dlg)
        {
            return HookRegistry.Register(ref calculatePressureOverride, dlg, "CalculatePressure");
        }

        public static bool UnregisterCalculatePressureOverride(voidDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculatePressureOverride, dlg, "CalculatePressure");
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
        private static voidDelegate[] calculateSunBodyFluxPreHandlers = NoHandlers;
        private static voidDelegate[] calculateSunBodyFluxPostHandlers = NoHandlers;

        public static bool RegisterCalculateSunBodyFluxOverride(voidDelegate dlg)
        {
            return HookRegistry.Register(ref calculateSunBodyFluxOverride, dlg, "CalculateSunBodyFlux");
        }

        public static bool UnregisterCalculateSunBodyFluxOverride(voidDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateSunBodyFluxOverride, dlg, "CalculateSunBodyFlux");
        }

        /// <summary>
        /// Adds a handler that runs before the sun and body flux calculation, once per frame (Update,
        /// through ThermoPrecalculate), not per physics step. Handlers run in registration order, each in
        /// its own try/catch, and a handler is only added once.
        /// </summary>
        public static void RegisterCalculateSunBodyFluxPre(voidDelegate dlg)
        {
            HookRegistry.Add(ref calculateSunBodyFluxPre, dlg);
            calculateSunBodyFluxPreHandlers = HandlersOf(calculateSunBodyFluxPre);
        }

        public static void UnregisterCalculateSunBodyFluxPre(voidDelegate dlg)
        {
            HookRegistry.Remove(ref calculateSunBodyFluxPre, dlg);
            calculateSunBodyFluxPreHandlers = HandlersOf(calculateSunBodyFluxPre);
        }

        /// <summary>
        /// Adds a handler that runs after the sun and body flux calculation, once per frame (Update).
        /// Handlers run in registration order, each in its own try/catch, and a handler is only added once.
        /// </summary>
        public static void RegisterCalculateSunBodyFluxPost(voidDelegate dlg)
        {
            HookRegistry.Add(ref calculateSunBodyFluxPost, dlg);
            calculateSunBodyFluxPostHandlers = HandlersOf(calculateSunBodyFluxPost);
        }

        public static void UnregisterCalculateSunBodyFluxPost(voidDelegate dlg)
        {
            HookRegistry.Remove(ref calculateSunBodyFluxPost, dlg);
            calculateSunBodyFluxPostHandlers = HandlersOf(calculateSunBodyFluxPost);
        }

        protected override void CalculateSunBodyFlux()
        {
            InvokeHandlers(calculateSunBodyFluxPreHandlers, "CalculateSunBodyFluxPre");

            if (calculateSunBodyFluxOverride == null)
            {
                base.CalculateSunBodyFlux();
            }
            else
            {
                calculateSunBodyFluxOverride(this);
            }

            InvokeHandlers(calculateSunBodyFluxPostHandlers, "CalculateSunBodyFluxPost");
        }

        public void BaseFICalculateSunBodyFlux()
        {
            base.CalculateSunBodyFlux();
        }


        // TODO : CalculateDensityThermalLerp
        private static doubleDelegate calculateDensityThermalLerpOverride;

        public static bool RegisterCalculateDensityThermalLerpOverride(doubleDelegate dlg)
        {
            return HookRegistry.Register(ref calculateDensityThermalLerpOverride, dlg, "CalculateDensityThermalLerp");
        }

        public static bool UnregisterCalculateDensityThermalLerpOverride(doubleDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateDensityThermalLerpOverride, dlg, "CalculateDensityThermalLerp");
        }

        public override double CalculateDensityThermalLerp()
        {
            if (calculateDensityThermalLerpOverride == null)
            {
                return base.CalculateDensityThermalLerp();
            }
            else
            {
                return calculateDensityThermalLerpOverride(this);
            }
        }

        public double BaseFICalculateDensityThermalLerp()
        {
            return base.CalculateDensityThermalLerp();
        }

        // TODO : CalculateBackgroundRadiationTemperature
        private static doubleDoubleDelegate calculateBackgroundRadiationTemperatureOverride;

        public static bool RegisterCalculateBackgroundRadiationTemperatureOverride(doubleDoubleDelegate dlg)
        {
            return HookRegistry.Register(ref calculateBackgroundRadiationTemperatureOverride, dlg, "CalculateBackgroundRadiationTemperature");
        }

        public static bool UnregisterCalculateBackgroundRadiationTemperatureOverride(doubleDoubleDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateBackgroundRadiationTemperatureOverride, dlg, "CalculateBackgroundRadiationTemperature");
        }

        protected override double CalculateBackgroundRadiationTemperature(double ambientTemp)
        {
            if (calculateBackgroundRadiationTemperatureOverride == null)
            {
                return base.CalculateBackgroundRadiationTemperature(ambientTemp);
            }
            else
            {
                return calculateBackgroundRadiationTemperatureOverride(this, ambientTemp);
            }
        }

        public double BaseFICalculateBackgroundRadiationTemperature(double ambientTemp)
        {
            return base.CalculateBackgroundRadiationTemperature(ambientTemp);
        }

        // TODO : CalculateConstantsVacuum
        private static voidDelegate calculateConstantsVacuumOverride;

        public static bool RegisterCalculateConstantsVacuumOverride(voidDelegate dlg)
        {
            return HookRegistry.Register(ref calculateConstantsVacuumOverride, dlg, "CalculateConstantsVacuum");
        }

        public static bool UnregisterCalculateConstantsVacuumOverride(voidDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateConstantsVacuumOverride, dlg, "CalculateConstantsVacuum");
        }

        protected override void CalculateConstantsVacuum()
        {
            if (calculateConstantsVacuumOverride == null)
            {
                base.CalculateConstantsVacuum();
            }
            else
            {
                calculateConstantsVacuumOverride(this);
            }
        }

        public void BaseFICalculateConstantsVacuum()
        {
            base.CalculateConstantsVacuum();
        }

        // TODO : CalculateConstantsAtmosphere
        private static voidDelegate calculateConstantsAtmosphereOverride;

        public static bool RegisterCalculateConstantsAtmosphereOverride(voidDelegate dlg)
        {
            return HookRegistry.Register(ref calculateConstantsAtmosphereOverride, dlg, "CalculateConstantsAtmosphere");
        }

        [Obsolete("Use RegisterCalculateConstantsAtmosphereOverride (capital C).")]
        public static bool RegistercalculateConstantsAtmosphereOverride(voidDelegate dlg)
        {
            return RegisterCalculateConstantsAtmosphereOverride(dlg);
        }

        public static bool UnregisterCalculateConstantsAtmosphereOverride(voidDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateConstantsAtmosphereOverride, dlg, "CalculateConstantsAtmosphere");
        }

        protected override void CalculateConstantsAtmosphere()
        {
            if (calculateConstantsAtmosphereOverride == null)
            {
                base.CalculateConstantsAtmosphere();
            }
            else
            {
                calculateConstantsAtmosphereOverride(this);
            }
        }

        public void BaseFICalculateConstantsAtmosphere()
        {
            base.CalculateConstantsAtmosphere();
        }

        // TODO : CalculateConvectiveCoefficient
        private static doubleDelegate calculateConvectiveCoefficientOverride;

        public static bool RegisterCalculateConvectiveCoefficientOverride(doubleDelegate dlg)
        {
            return HookRegistry.Register(ref calculateConvectiveCoefficientOverride, dlg, "CalculateConvectiveCoefficient");
        }

        public static bool UnregisterCalculateConvectiveCoefficientOverride(doubleDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateConvectiveCoefficientOverride, dlg, "CalculateConvectiveCoefficient");
        }

        protected override double CalculateConvectiveCoefficient()
        {
            if (calculateConvectiveCoefficientOverride == null)
            {
                return base.CalculateConvectiveCoefficient();
            }
            else
            {
                return calculateConvectiveCoefficientOverride(this);
            }
        }

        public double BaseFICalculateConvectiveCoefficient()
        {
            return base.CalculateConvectiveCoefficient();
        }

        
        // TODO : CalculateConvectiveCoefficientNewtonian
        private static doubleDelegate calculateConvectiveCoefficientNewtonianOverride;

        public static bool RegisterCalculateConvectiveCoefficientNewtonianOverride(doubleDelegate dlg)
        {
            return HookRegistry.Register(ref calculateConvectiveCoefficientNewtonianOverride, dlg, "CalculateConvectiveCoefficientNewtonian");
        }

        public static bool UnregisterCalculateConvectiveCoefficientNewtonianOverride(doubleDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateConvectiveCoefficientNewtonianOverride, dlg, "CalculateConvectiveCoefficientNewtonian");
        }

        protected override double CalculateConvectiveCoefficientNewtonian()
        {
            if (calculateConvectiveCoefficientNewtonianOverride == null)
            {
                return base.CalculateConvectiveCoefficientNewtonian();
            }
            else
            {
                return calculateConvectiveCoefficientNewtonianOverride(this);
            }
        }

        public double BaseFICalculateConvectiveCoefficientNewtonian()
        {
            return base.CalculateConvectiveCoefficientNewtonian();
        }

        // TODO : CalculateConvectiveCoefficientMach
        private static doubleDelegate calculateConvectiveCoefficientMachOverride;

        public static bool RegisterCalculateConvectiveCoefficientMachOverride(doubleDelegate dlg)
        {
            return HookRegistry.Register(ref calculateConvectiveCoefficientMachOverride, dlg, "CalculateConvectiveCoefficientMach");
        }

        public static bool UnregisterCalculateConvectiveCoefficientMachOverride(doubleDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateConvectiveCoefficientMachOverride, dlg, "CalculateConvectiveCoefficientMach");
        }

        protected override double CalculateConvectiveCoefficientMach()
        {
            if (calculateConvectiveCoefficientMachOverride == null)
            {
                return base.CalculateConvectiveCoefficientMach();
            }
            else
            {
                return calculateConvectiveCoefficientMachOverride(this);
            }
        }

        public double BaseFICalculateConvectiveCoefficientMach()
        {
            return base.CalculateConvectiveCoefficientMach();
        }

        private static voidPartDelegate updateAerodynamicsOverride;

        public static bool RegisterUpdateAerodynamicsOverride(voidPartDelegate dlg)
        {
            return HookRegistry.Register(ref updateAerodynamicsOverride, dlg, "UpdateAerodynamics");
        }

        public static bool UnregisterUpdateAerodynamicsOverride(voidPartDelegate dlg)
        {
            return HookRegistry.Unregister(ref updateAerodynamicsOverride, dlg, "UpdateAerodynamics");
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
            return HookRegistry.Register(ref calculateDragValueOverride, dlg, "CalculateDragValue");
        }

        public static bool UnregisterCalculateDragValueOverride(doublePartDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateDragValueOverride, dlg, "CalculateDragValue");
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
            return HookRegistry.Register(ref updateThermalGraphOverride, dlg, "UpdateThermalGraph");
        }

        public static bool UnregisterUpdateThermalGraphOverride(voidDelegate dlg)
        {
            return HookRegistry.Unregister(ref updateThermalGraphOverride, dlg, "UpdateThermalGraph");
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

        public static bool RegisterUpdateCompoundPartsOverride(voidDelegate dlg)
        {
            return HookRegistry.Register(ref updateCompoundPartsOverride, dlg, "UpdateCompoundParts");
        }

        public static bool UnregisterUpdateCompoundPartsOverride(voidDelegate dlg)
        {
            return HookRegistry.Unregister(ref updateCompoundPartsOverride, dlg, "UpdateCompoundParts");
        }

        [Obsolete("Use RegisterUpdateCompoundPartsOverride.")]
        public static bool RegisterUpdateCompoundParts(voidDelegate dlg)
        {
            return RegisterUpdateCompoundPartsOverride(dlg);
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

        public static bool RegisterSetSkinPropertiesOverride(voidThermalDataDelegate dlg)
        {
            return HookRegistry.Register(ref setSkinPropertiesOverride, dlg, "SetSkinProperties");
        }

        public static bool UnregisterSetSkinPropertiesOverride(voidThermalDataDelegate dlg)
        {
            return HookRegistry.Unregister(ref setSkinPropertiesOverride, dlg, "SetSkinProperties");
        }

        [Obsolete("Use RegisterSetSkinPropertiesOverride.")]
        public static bool RegisterSetSkinProperties(voidThermalDataDelegate dlg)
        {
            return RegisterSetSkinPropertiesOverride(dlg);
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

        public void BaseFISetSkinProperties(PartThermalData ptd)
        {
            base.SetSkinProperties(ptd);
        }

        [Obsolete("Use BaseFISetSkinProperties.")]
        public void BaseFIetSkinPropertie(PartThermalData ptd)
        {
            base.SetSkinProperties(ptd);
        }

        private static voidDelegate updateConductionOverride;

        public static bool RegisterUpdateConductionOverride(voidDelegate dlg)
        {
            return HookRegistry.Register(ref updateConductionOverride, dlg, "UpdateConduction");
        }

        public static bool UnregisterUpdateConductionOverride(voidDelegate dlg)
        {
            return HookRegistry.Unregister(ref updateConductionOverride, dlg, "UpdateConduction");
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
            return HookRegistry.Register(ref updateConvectionOverride, dlg, "UpdateConvection");
        }

        public static bool UnregisterUpdateConvectionOverride(voidThermalDataDelegate dlg)
        {
            return HookRegistry.Unregister(ref updateConvectionOverride, dlg, "UpdateConvection");
        }

        public override void UpdateConvection(PartThermalData ptd)
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
            return HookRegistry.Register(ref updateRadiationOverride, dlg, "UpdateRadiation");
        }

        public static bool UnregisterUpdateRadiationOverride(voidThermalDataDelegate dlg)
        {
            return HookRegistry.Unregister(ref updateRadiationOverride, dlg, "UpdateRadiation");
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

        private static voidDelegate updateMassStatsOverride;

        public static bool RegisterUpdateMassStatsOverride(voidDelegate dlg)
        {
            return HookRegistry.Register(ref updateMassStatsOverride, dlg, "UpdateMassStats");
        }

        public static bool UnregisterUpdateMassStatsOverride(voidDelegate dlg)
        {
            return HookRegistry.Unregister(ref updateMassStatsOverride, dlg, "UpdateMassStats");
        }

        protected override void UpdateMassStats()
        {
            if (updateMassStatsOverride == null)
            {
                base.UpdateMassStats();
            }
            else
            {
                updateMassStatsOverride(this);
            }
        }

        public void BaseFIUpdateMassStats()
        {
            base.UpdateMassStats();
        }

        private static doubleThermalDataDelegate updateGetSunAreaOverride;

        public static bool RegisterGetSunAreaOverride(doubleThermalDataDelegate dlg)
        {
            return HookRegistry.Register(ref updateGetSunAreaOverride, dlg, "GetSunArea");
        }

        public static bool UnregisterGetSunAreaOverride(doubleThermalDataDelegate dlg)
        {
            return HookRegistry.Unregister(ref updateGetSunAreaOverride, dlg, "GetSunArea");
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
            return HookRegistry.Register(ref getBodyAreaOverride, dlg, "GetBodyArea");
        }

        public static bool UnregisterGetBodyAreaOverride(doubleThermalDataDelegate dlg)
        {
            return HookRegistry.Unregister(ref getBodyAreaOverride, dlg, "GetBodyArea");
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

        public double BaseFIGetBodyArea(PartThermalData ptd)
        {
            return base.GetBodyArea(ptd);
        }

        [Obsolete("Use BaseFIGetBodyArea.")]
        public double BaseFIBodyArea(PartThermalData ptd)
        {
            return base.GetBodyArea(ptd);
        }






        private static doublePartDelegate calculateAerodynamicAreaOverride;

        public static bool RegisterCalculateAerodynamicAreaOverride(doublePartDelegate dlg)
        {
            return HookRegistry.Register(ref calculateAerodynamicAreaOverride, dlg, "CalculateAerodynamicArea");
        }

        public static bool UnregisterCalculateAerodynamicAreaOverride(doublePartDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateAerodynamicAreaOverride, dlg, "CalculateAerodynamicArea");
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
            return HookRegistry.Register(ref calculateAreaRadiativeOverride, dlg, "CalculateAreaRadiative");
        }

        public static bool UnregisterCalculateAreaRadiativeOverride(doublePartDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateAreaRadiativeOverride, dlg, "CalculateAreaRadiative");
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
            return HookRegistry.Register(ref calculateAreaExposedOverride, dlg, "CalculateAreaExposed");
        }

        public static bool UnregisterCalculateAreaExposedOverride(doublePartDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateAreaExposedOverride, dlg, "CalculateAreaExposed");
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

        public double BaseFIGetPhysicslessChildMass(Part part)
        {
            return base.GetPhysicslessChildMass(part);
        }

        // Hooks added in 1.3 for the remaining virtual methods of the stock integrator.

        private static voidBoolDelegate thermalIntegrationPassOverride;

        public static bool RegisterThermalIntegrationPassOverride(voidBoolDelegate dlg)
        {
            return HookRegistry.Register(ref thermalIntegrationPassOverride, dlg, "ThermalIntegrationPass");
        }

        public static bool UnregisterThermalIntegrationPassOverride(voidBoolDelegate dlg)
        {
            return HookRegistry.Unregister(ref thermalIntegrationPassOverride, dlg, "ThermalIntegrationPass");
        }

        /// <summary>
        /// One pass of the thermal integration: conduction, then convection and radiation per part. Called one or more times per thermal step by the stock UpdateThermodynamics.
        /// </summary>
        public override void ThermalIntegrationPass(bool averageWithPrevious)
        {
            if (thermalIntegrationPassOverride == null)
            {
                base.ThermalIntegrationPass(averageWithPrevious);
            }
            else
            {
                thermalIntegrationPassOverride(this, averageWithPrevious);
            }
        }

        public void BaseFIThermalIntegrationPass(bool averageWithPrevious)
        {
            base.ThermalIntegrationPass(averageWithPrevious);
        }

        private static voidThermalDataDelegate precalcConductionOverride;

        public static bool RegisterPrecalcConductionOverride(voidThermalDataDelegate dlg)
        {
            return HookRegistry.Register(ref precalcConductionOverride, dlg, "PrecalcConduction");
        }

        public static bool UnregisterPrecalcConductionOverride(voidThermalDataDelegate dlg)
        {
            return HookRegistry.Unregister(ref precalcConductionOverride, dlg, "PrecalcConduction");
        }

        /// <summary>
        /// Per-part conduction coefficients, computed once per thermal step before the passes.
        /// </summary>
        public override void PrecalcConduction(PartThermalData ptd)
        {
            if (precalcConductionOverride == null)
            {
                base.PrecalcConduction(ptd);
            }
            else
            {
                precalcConductionOverride(this, ptd);
            }
        }

        public void BaseFIPrecalcConduction(PartThermalData ptd)
        {
            base.PrecalcConduction(ptd);
        }

        private static voidThermalDataDelegate precalcConvectionOverride;

        public static bool RegisterPrecalcConvectionOverride(voidThermalDataDelegate dlg)
        {
            return HookRegistry.Register(ref precalcConvectionOverride, dlg, "PrecalcConvection");
        }

        public static bool UnregisterPrecalcConvectionOverride(voidThermalDataDelegate dlg)
        {
            return HookRegistry.Unregister(ref precalcConvectionOverride, dlg, "PrecalcConvection");
        }

        /// <summary>
        /// Per-part convection coefficient and post-shock temperature, computed once per thermal step before the passes.
        /// </summary>
        public override void PrecalcConvection(PartThermalData ptd)
        {
            if (precalcConvectionOverride == null)
            {
                base.PrecalcConvection(ptd);
            }
            else
            {
                precalcConvectionOverride(this, ptd);
            }
        }

        public void BaseFIPrecalcConvection(PartThermalData ptd)
        {
            base.PrecalcConvection(ptd);
        }

        private static voidThermalDataDelegate precalcRadiationOverride;

        public static bool RegisterPrecalcRadiationOverride(voidThermalDataDelegate dlg)
        {
            return HookRegistry.Register(ref precalcRadiationOverride, dlg, "PrecalcRadiation");
        }

        public static bool UnregisterPrecalcRadiationOverride(voidThermalDataDelegate dlg)
        {
            return HookRegistry.Unregister(ref precalcRadiationOverride, dlg, "PrecalcRadiation");
        }

        /// <summary>
        /// Per-part radiative coefficients and incoming fluxes, computed once per thermal step before the passes.
        /// </summary>
        public override void PrecalcRadiation(PartThermalData ptd)
        {
            if (precalcRadiationOverride == null)
            {
                base.PrecalcRadiation(ptd);
            }
            else
            {
                precalcRadiationOverride(this, ptd);
            }
        }

        public void BaseFIPrecalcRadiation(PartThermalData ptd)
        {
            base.PrecalcRadiation(ptd);
        }

        private static voidThermalDataDelegate unifySkinTempOverride;

        public static bool RegisterUnifySkinTempOverride(voidThermalDataDelegate dlg)
        {
            return HookRegistry.Register(ref unifySkinTempOverride, dlg, "UnifySkinTemp");
        }

        public static bool UnregisterUnifySkinTempOverride(voidThermalDataDelegate dlg)
        {
            return HookRegistry.Unregister(ref unifySkinTempOverride, dlg, "UnifySkinTemp");
        }

        /// <summary>
        /// Merges the exposed and unexposed skin temperatures of a part.
        /// </summary>
        public override void UnifySkinTemp(PartThermalData ptd)
        {
            if (unifySkinTempOverride == null)
            {
                base.UnifySkinTemp(ptd);
            }
            else
            {
                unifySkinTempOverride(this, ptd);
            }
        }

        public void BaseFIUnifySkinTemp(PartThermalData ptd)
        {
            base.UnifySkinTemp(ptd);
        }

        private static voidPartDelegate setSkinThermalMassOverride;

        public static bool RegisterSetSkinThermalMassOverride(voidPartDelegate dlg)
        {
            return HookRegistry.Register(ref setSkinThermalMassOverride, dlg, "SetSkinThermalMass");
        }

        public static bool UnregisterSetSkinThermalMassOverride(voidPartDelegate dlg)
        {
            return HookRegistry.Unregister(ref setSkinThermalMassOverride, dlg, "SetSkinThermalMass");
        }

        /// <summary>
        /// Sets the skin thermal mass of a part from its mass and skin fraction. Called from UpdateMassStats.
        /// </summary>
        public override void SetSkinThermalMass(Part part)
        {
            if (setSkinThermalMassOverride == null)
            {
                base.SetSkinThermalMass(part);
            }
            else
            {
                setSkinThermalMassOverride(this, part);
            }
        }

        public void BaseFISetSkinThermalMass(Part part)
        {
            base.SetSkinThermalMass(part);
        }

        private static voidDelegate smoothVelocityOverride;

        public static bool RegisterSmoothVelocityOverride(voidDelegate dlg)
        {
            return HookRegistry.Register(ref smoothVelocityOverride, dlg, "SmoothVelocity");
        }

        public static bool UnregisterSmoothVelocityOverride(voidDelegate dlg)
        {
            return HookRegistry.Unregister(ref smoothVelocityOverride, dlg, "SmoothVelocity");
        }

        /// <summary>
        /// The velocity smoother that produces Vel from the raw rigidbody velocity every step.
        /// </summary>
        protected override void SmoothVelocity()
        {
            if (smoothVelocityOverride == null)
            {
                base.SmoothVelocity();
            }
            else
            {
                smoothVelocityOverride(this);
            }
        }

        public void BaseFISmoothVelocity()
        {
            base.SmoothVelocity();
        }

        private static voidDelegate thermoPrecalculateOverride;

        public static bool RegisterThermoPrecalculateOverride(voidDelegate dlg)
        {
            return HookRegistry.Register(ref thermoPrecalculateOverride, dlg, "ThermoPrecalculate");
        }

        public static bool UnregisterThermoPrecalculateOverride(voidDelegate dlg)
        {
            return HookRegistry.Unregister(ref thermoPrecalculateOverride, dlg, "ThermoPrecalculate");
        }

        /// <summary>
        /// The per-frame (Update) thermal work: in stock, CalculateSunBodyFlux.
        /// </summary>
        public override void ThermoPrecalculate()
        {
            if (thermoPrecalculateOverride == null)
            {
                base.ThermoPrecalculate();
            }
            else
            {
                thermoPrecalculateOverride(this);
            }
        }

        public void BaseFIThermoPrecalculate()
        {
            base.ThermoPrecalculate();
        }

        private static doublePartDelegate calculateDragValue_SphericalOverride;

        public static bool RegisterCalculateDragValue_SphericalOverride(doublePartDelegate dlg)
        {
            return HookRegistry.Register(ref calculateDragValue_SphericalOverride, dlg, "CalculateDragValue_Spherical");
        }

        public static bool UnregisterCalculateDragValue_SphericalOverride(doublePartDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateDragValue_SphericalOverride, dlg, "CalculateDragValue_Spherical");
        }

        /// <summary>
        /// Drag value of a part with the spherical drag model.
        /// </summary>
        protected override double CalculateDragValue_Spherical(Part part)
        {
            if (calculateDragValue_SphericalOverride == null)
            {
                return base.CalculateDragValue_Spherical(part);
            }
            else
            {
                return calculateDragValue_SphericalOverride(this, part);
            }
        }

        public double BaseFICalculateDragValue_Spherical(Part part)
        {
            return base.CalculateDragValue_Spherical(part);
        }

        private static doublePartDelegate calculateDragValue_CylindricalOverride;

        public static bool RegisterCalculateDragValue_CylindricalOverride(doublePartDelegate dlg)
        {
            return HookRegistry.Register(ref calculateDragValue_CylindricalOverride, dlg, "CalculateDragValue_Cylindrical");
        }

        public static bool UnregisterCalculateDragValue_CylindricalOverride(doublePartDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateDragValue_CylindricalOverride, dlg, "CalculateDragValue_Cylindrical");
        }

        /// <summary>
        /// Drag value of a part with the cylindrical drag model.
        /// </summary>
        protected override double CalculateDragValue_Cylindrical(Part part)
        {
            if (calculateDragValue_CylindricalOverride == null)
            {
                return base.CalculateDragValue_Cylindrical(part);
            }
            else
            {
                return calculateDragValue_CylindricalOverride(this, part);
            }
        }

        public double BaseFICalculateDragValue_Cylindrical(Part part)
        {
            return base.CalculateDragValue_Cylindrical(part);
        }

        private static doublePartDelegate calculateDragValue_ConicOverride;

        public static bool RegisterCalculateDragValue_ConicOverride(doublePartDelegate dlg)
        {
            return HookRegistry.Register(ref calculateDragValue_ConicOverride, dlg, "CalculateDragValue_Conic");
        }

        public static bool UnregisterCalculateDragValue_ConicOverride(doublePartDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateDragValue_ConicOverride, dlg, "CalculateDragValue_Conic");
        }

        /// <summary>
        /// Drag value of a part with the conic drag model.
        /// </summary>
        protected override double CalculateDragValue_Conic(Part part)
        {
            if (calculateDragValue_ConicOverride == null)
            {
                return base.CalculateDragValue_Conic(part);
            }
            else
            {
                return calculateDragValue_ConicOverride(this, part);
            }
        }

        public double BaseFICalculateDragValue_Conic(Part part)
        {
            return base.CalculateDragValue_Conic(part);
        }

        private static doublePartDelegate calculateDragValue_CubeOverride;

        public static bool RegisterCalculateDragValue_CubeOverride(doublePartDelegate dlg)
        {
            return HookRegistry.Register(ref calculateDragValue_CubeOverride, dlg, "CalculateDragValue_Cube");
        }

        public static bool UnregisterCalculateDragValue_CubeOverride(doublePartDelegate dlg)
        {
            return HookRegistry.Unregister(ref calculateDragValue_CubeOverride, dlg, "CalculateDragValue_Cube");
        }

        /// <summary>
        /// Drag value of a part from its drag cubes (the stock default).
        /// </summary>
        protected override double CalculateDragValue_Cube(Part part)
        {
            if (calculateDragValue_CubeOverride == null)
            {
                return base.CalculateDragValue_Cube(part);
            }
            else
            {
                return calculateDragValue_CubeOverride(this, part);
            }
        }

        public double BaseFICalculateDragValue_Cube(Part part)
        {
            return base.CalculateDragValue_Cube(part);
        }

        static void print(string msg)
        {
            MonoBehaviour.print("[ModularFlightIntegrator] " + msg);
        }


    }

}
