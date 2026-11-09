using System;
using UnityEngine;

namespace ModularFI
{
    [DefaultExecutionOrder(-102)]
    public class ModularVesselPrecalculate : VesselPrecalculate
    {
        // Universal time of the last MainPhysics run, so that a second call in the
        // same physics step does nothing (Principia calls MainPhysics itself before
        // Unity does). The stock Update uses the same key for the same purpose.
        private double lastMainPhysicsUT = double.NaN;

        private static Action runFirstOverride;

        public static bool RegisterRunFirstOverride(Action act)
        {
            return HookRegistry.Register(ref runFirstOverride, act, "RunFirst");
        }

        public static bool UnregisterRunFirstOverride(Action act)
        {
            return HookRegistry.Unregister(ref runFirstOverride, act, "RunFirst");
        }

        [Obsolete("This overload registers the RunFirst override, not MainPhysics. Use RegisterRunFirstOverride.")]
        public static bool RegisterMainPhysicsOverride(Action act)
        {
            return RegisterRunFirstOverride(act);
        }

        /// <summary>
        /// Run when a vessel initializes in flight
        /// </summary>
        public override void RunFirst()
        {
            if (runFirstOverride != null)
                runFirstOverride();
            else
                base.RunFirst();
        }

        public void BaseRunFirst()
        {
            base.RunFirst();
        }

        private static Action<bool> mainPhysicsOverride;
        
        public static bool RegisterMainPhysicsOverride(Action<bool> act)
        {
            return HookRegistry.Register(ref mainPhysicsOverride, act, "MainPhysics");
        }

        public static bool UnregisterMainPhysicsOverride(Action<bool> act)
        {
            return HookRegistry.Unregister(ref mainPhysicsOverride, act, "MainPhysics");
        }
        
        /// <summary>
        /// Does the main physics calls. Run from fixed update. Bits run in Update.
        /// </summary>
        /// <param name="doKillChecks">Do we check if the vessel should be killed?</param>
        public override void MainPhysics(bool doKillChecks)
        {
            double ut = Planetarium.GetUniversalTime();
            if (ut == lastMainPhysicsUT)
                return;

            if (mainPhysicsOverride != null)
                mainPhysicsOverride(doKillChecks);
            else
                base.MainPhysics(doKillChecks);

            lastMainPhysicsUT = ut;
        }

        public void BaseMainPhysics(bool doKillChecks)
        {
            base.MainPhysics(doKillChecks);
        }

        private static Action applyVelocityCorrectionOverride;

        public static bool RegisterApplyVelocityCorrectionOverride(Action act)
        {
            return HookRegistry.Register(ref applyVelocityCorrectionOverride, act, "ApplyVelocityCorrection");
        }

        public static bool UnregisterApplyVelocityCorrectionOverride(Action act)
        {
            return HookRegistry.Unregister(ref applyVelocityCorrectionOverride, act, "ApplyVelocityCorrection");
        }

        /// <summary>
        /// Applies the velocity correction from drift compensation (if any).
        /// </summary>
        public override void ApplyVelocityCorrection()
        {
            if (applyVelocityCorrectionOverride != null)
                applyVelocityCorrectionOverride();
            else
                base.ApplyVelocityCorrection();
        }

        public void BaseApplyVelocityCorrection()
        {
            base.ApplyVelocityCorrection();
        }

        private static Action goOnRailsOverride;

        public static bool RegisterGoOnRailsOverride(Action act)
        {
            return HookRegistry.Register(ref goOnRailsOverride, act, "GoOnRails");
        }

        public static bool UnregisterGoOnRailsOverride(Action act)
        {
            return HookRegistry.Unregister(ref goOnRailsOverride, act, "GoOnRails");
        }

        /// <summary>
        /// Called by Vessel.GoOnRails. Clears any post-integration velocity offset.
        /// </summary>
        public override void GoOnRails()
        {
            if (goOnRailsOverride != null)
                goOnRailsOverride();
            else
                base.GoOnRails();
        }

        public void BaseGoOnRails()
        {
            base.GoOnRails();
        }

        private static Action goOffRailsOverride;

        public static bool RegisterGoOffRailsOverride(Action act)
        {
            return HookRegistry.Register(ref goOffRailsOverride, act, "GoOffRails");
        }

        public static bool UnregisterGoOffRailsOverride(Action act)
        {
            return HookRegistry.Unregister(ref goOffRailsOverride, act, "GoOffRails");
        }

        /// <summary>
        /// Called by Vessel.GoOffRails
        /// </summary>
        public override void GoOffRails()
        {
            if (goOffRailsOverride != null)
                goOffRailsOverride();
            else
                base.GoOffRails();
        }

        public void BaseGoOffRails()
        {
            base.GoOffRails();
        }

        //protected override void StartEasing()
        //{
        //    base.StartEasing();
        //}
        //
        //protected override void StopEasing()
        //{
        //    base.StopEasing();
        //}

        private static Action calculateGravityOverride;

        public static bool RegisterCalculateGravityOverride(Action act)
        {
            return HookRegistry.Register(ref calculateGravityOverride, act, "CalculateGravity");
        }

        public static bool UnregisterCalculateGravityOverride(Action act)
        {
            return HookRegistry.Unregister(ref calculateGravityOverride, act, "CalculateGravity");
        }

        /// <summary>
        /// Calculate the force of gravity, using drift compensation if in orbit and the compensation is better than not.
        /// Ease gravity coming off rails while landed/splashed.
        /// </summary>
        public override void CalculateGravity()
        {
            if (calculateGravityOverride != null)
                calculateGravityOverride();
            else
                base.CalculateGravity();
        }

        public void BaseCalculateGravity()
        {
            base.CalculateGravity();
        }

        ///// <summary>
        ///// This will update pos/rot if landed/splashed and on rails
        ///// </summary>
        //public override void SetLandedPosRot()
        //{
        //    base.SetLandedPosRot();
        //}
        
        private static Action calculatePhysicsStatsOverride;

        public static bool RegisterCalculatePhysicsStatsOverride(Action act)
        {
            return HookRegistry.Register(ref calculatePhysicsStatsOverride, act, "CalculatePhysicsStats");
        }

        public static bool UnregisterCalculatePhysicsStatsOverride(Action act)
        {
            return HookRegistry.Unregister(ref calculatePhysicsStatsOverride, act, "CalculatePhysicsStats");
        }

        /// <summary>
        /// Will calculate center of mass and total mass, mass-weighted average linear and angular velocity, moment of inertia, and angular momentum
        /// Depends on FlightIntegrator setting part.physicsMass.
        /// If vessel is unloaded, use traditional calculations based off stored values and root part.
        /// </summary>
        public override void CalculatePhysicsStats()
        {
            if (calculatePhysicsStatsOverride != null)
                calculatePhysicsStatsOverride();
            else
                base.CalculatePhysicsStats();
        }

        public void BaseCalculatePhysicsStats()
        {
            base.CalculatePhysicsStats();
        }

        //public override bool isEasingGravity { get; set; }
    }
}
