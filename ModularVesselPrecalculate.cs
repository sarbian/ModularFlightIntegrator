using System;

namespace ModularFI
{
    class ModularVesselPrecalculate : VesselPrecalculate
    {
        public override void Awake()
        {
            TimingManager.UpdateAdd(TimingManager.TimingStage.Precalc, TimedUpdate);
            TimingManager.FixedUpdateAdd(TimingManager.TimingStage.Precalc, TimedFixedUpdate);

            base.Awake();
        }

        public override void OnDestroy()
        {
            TimingManager.UpdateRemove(TimingManager.TimingStage.Precalc, TimedUpdate);
            TimingManager.FixedUpdateRemove(TimingManager.TimingStage.Precalc, TimedFixedUpdate);
            base.OnDestroy();
        }

        public new void FixedUpdate()
        {
            // Empty on puprose. See ModularFlightIntegrator FixedUpdate comment
        }

        public void TimedFixedUpdate()
        {
            if (gameObject.activeInHierarchy && this.enabled)
                base.FixedUpdate();
        }

        public override void Update()
        {
            // Empty on puprose. See ModularFlightIntegrator FixedUpdate comment
        }

        /// <summary>
        /// Used for loaded/unpacked vessels so display matches physics. Runs most of what FixedUpdate does.
        /// However, it does so with zero time offset and does not calc gravity (that happens in next fixed frame).
        /// Finally, it signals to next fixed frame it doesn't have to rerun this stuff
        /// </summary>
        public void TimedUpdate()
        {
            if (gameObject.activeInHierarchy && this.enabled)
                base.Update();
        }

        private static Action runFirstOverride;

        public static bool RegisterMainPhysicsOverride(Action act)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }

            if (runFirstOverride == null)
            {
                runFirstOverride = act;
                return true;
            }

            print("RunFirst already has an override");
            return false;
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

        private static Action<bool> mainPhysicsOverride;
        
        public static bool RegisterMainPhysicsOverride(Action<bool> act)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }

            if (mainPhysicsOverride == null)
            {
                mainPhysicsOverride = act;
                return true;
            }

            print("MainPhysics already has an override");
            return false;
        }

        /// <summary>
        /// Does the main physics calls. Run from fixed update. Bits run in Update.
        /// </summary>
        /// <param name="doKillChecks">Do we check if the vessel should be killed?</param>
        public override void MainPhysics(bool doKillChecks)
        {
            if (mainPhysicsOverride != null)
                mainPhysicsOverride(doKillChecks);
            else
                base.MainPhysics(doKillChecks);
        }

        private static Action applyVelocityCorrectionOverride;

        public static bool RegisterApplyVelocityCorrectionOverride(Action act)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }

            if (applyVelocityCorrectionOverride == null)
            {
                applyVelocityCorrectionOverride = act;
                return true;
            }

            print("ApplyVelocityCorrection already has an override");
            return false;
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

        private static Action goOnRailsOverride;

        public static bool RegisterGoOnRailsOverride(Action act)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }

            if (goOnRailsOverride == null)
            {
                goOnRailsOverride = act;
                return true;
            }

            print("GoOnRails already has an override");
            return false;
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

        private static Action goOffRailsOverride;

        public static bool RegisterGoOffRailsOverride(Action act)
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }

            if (goOffRailsOverride == null)
            {
                goOffRailsOverride = act;
                return true;
            }

            print("GoOffRails already has an override");
            return false;
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
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }

            if (calculateGravityOverride == null)
            {
                calculateGravityOverride = act;
                return true;
            }

            print("CalculateGravity already has an override");
            return false;
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
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                print("You can only register on the SPACECENTER scene");
            }

            if (calculatePhysicsStatsOverride == null)
            {
                calculatePhysicsStatsOverride = act;
                return true;
            }

            print("CalculatePhysicsStats already has an override");
            return false;
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

        //public override bool isEasingGravity { get; set; }
    }
}
