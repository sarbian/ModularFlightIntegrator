using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ModularFlightIntegrator : FlightIntegrator
{

    public delegate void voidDelegate();
    public delegate void voidPartDelegate(Part part);
    public delegate double doublePartDelegate(Part part);
    public delegate void voidThermalDataDelegate(PartThermalData ptd);
    public delegate double IntegratePhysicalObjectsDelegate(List<GameObject> pObjs, double atmDensity);


    protected void Awake()
    {
        VesselModuleManager.RemoveModuleOfType(typeof (FlightIntegrator));
        string msg = "ModularFlightIntegrator Awake. Current modules coVesselModule : \n";
        foreach (var vesselModuleWrapper in VesselModuleManager.GetModules(false, true))
        {
            msg += "  " + vesselModuleWrapper.type.ToString() + " active=" + vesselModuleWrapper.active + " order=" + vesselModuleWrapper.order + "\n";
        }
        MonoBehaviour.print(msg);

    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void HookVesselEvents()
    {
        base.HookVesselEvents();
    }

    protected override void UnhookVesselEvents()
    {
        base.UnhookVesselEvents();
    }

    protected override void FixedUpdate()
    {
        // UpdateThermodynamics

        // UpdateOcclusion

        // Integrate

        // IntegratePhysicalObjects

        base.FixedUpdate();
    }

    private static voidDelegate UpdateThermodynamicsOverride;
    private static voidDelegate UpdateThermodynamicsPre;
    private static voidDelegate UpdateThermodynamicsPost;

    public static bool RegisterUpdateThermodynamicsOverride(voidDelegate dlg)
    {
        if (UpdateThermodynamicsOverride == null)
        {
            UpdateThermodynamicsOverride = dlg;
            return true;
        }

        print("UpdateThermodynamics already has an override");
        return false;
    }

    public static void RegisterUpdateThermodynamicsPre(voidDelegate dlg)
    {
        UpdateThermodynamicsPre += dlg;
    }

    public static void RegisterUpdateThermodynamicsPost(voidDelegate dlg)
    {
        UpdateThermodynamicsPost += dlg;
    }

    protected override void UpdateThermodynamics()
    {
        // UpdateThermalGraph

        // UpdateConduction

        // UpdateConvection

        // UpdateRadiation

        if (UpdateThermodynamicsPre != null)
        {
            UpdateThermodynamicsPre();
        }

        if (UpdateThermodynamicsOverride == null)
        {
            base.UpdateThermodynamics();
        }
        else
        {
            UpdateThermodynamicsOverride();
        }

        if (UpdateThermodynamicsPost != null)
        {
            UpdateThermodynamicsPost();
        }

    }


    private static voidDelegate UpdateOcclusionOverride;

    protected override void UpdateOcclusion()
    {
        if (UpdateOcclusionOverride == null)
        {
            base.UpdateOcclusion();
        }
        else
        {
            UpdateOcclusionOverride();
        }
    }


    private static voidPartDelegate IntegrateOverride;
    protected override void Integrate(Part part)
    {
        // UpdateAerodynamics

        if (IntegrateOverride == null)
        {
            base.Integrate(part);
        }
        else
        {
            IntegrateOverride(part);
        }
    }


    private static IntegratePhysicalObjectsDelegate IntegratePhysicalObjectsOverride;
    protected override void IntegratePhysicalObjects(List<GameObject> pObjs, double atmDensity)
    {
        if (IntegratePhysicalObjectsOverride == null)
        {
            base.IntegratePhysicalObjects(pObjs, atmDensity);
        }
        else
        {
            IntegratePhysicalObjectsOverride(pObjs, atmDensity);
        }
    }

    private static voidPartDelegate UpdateAerodynamicsOverride;
    protected override void UpdateAerodynamics(Part part)
    {
        // CalculateDragValue

        // CalculateAerodynamicArea
        // CalculateAreaRadiative
        // CalculateAreaExposed

        if (UpdateAerodynamicsOverride == null)
        {
            base.UpdateAerodynamics(part);
        }
        else
        {
            UpdateAerodynamicsOverride(part);
        }
    }


    private static doublePartDelegate CalculateDragValueOverride;
    protected override double CalculateDragValue(Part part)
    {
        // CalculateDragValue_Spherical
        // CalculateDragValue_Cylindrical
        // CalculateDragValue_Conic
        // CalculateDragValue_Cube

        if (CalculateDragValueOverride == null)
        {
            return base.CalculateDragValue(part);
        }
        else
        {
            return CalculateDragValueOverride(part);
        }
    }

    private static voidDelegate UpdateThermalGraphOverride;
    protected override void UpdateThermalGraph()
    {
        if (UpdateThermalGraphOverride == null)
        {
            base.UpdateThermalGraph();
        }
        else
        {
            UpdateThermalGraphOverride();
        }
    }

    private static voidDelegate UpdateConductionOverride;
    protected override void UpdateConduction()
    {
        if (UpdateConductionOverride == null)
        {
            base.UpdateConduction();
        }
        else
        {
            UpdateConductionOverride();
        }
    }

    private static voidThermalDataDelegate UpdateConvectionOverride;
    protected override void UpdateConvection(PartThermalData ptd)
    {
        if (UpdateConvectionOverride == null)
        {
            base.UpdateConvection(ptd);
        }
        else
        {
            UpdateConvectionOverride(ptd);
        }
    }

    private static voidThermalDataDelegate UpdateRadiationOverride;
    protected override void UpdateRadiation(PartThermalData ptd)
    {
        if (UpdateRadiationOverride == null)
        {
            base.UpdateRadiation(ptd);
        }
        else
        {
            UpdateRadiationOverride(ptd);
        }
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

    private static doublePartDelegate CalculateAerodynamicAreaOverride;
    protected override double CalculateAerodynamicArea(Part part)
    {
        if (CalculateAerodynamicAreaOverride == null)
        {
            return base.CalculateAerodynamicArea(part);
        }
        else
        {
            return CalculateAerodynamicAreaOverride(part);
        }
    }

    private static doublePartDelegate CalculateAreaRadiativeOverride;
    protected override double CalculateAreaRadiative(Part part)
    {
        if (CalculateAreaRadiativeOverride == null)
        {
            return base.CalculateAreaRadiative(part);
        }
        else
        {
            return CalculateAreaRadiativeOverride(part);
        }
    }

    private static doublePartDelegate CalculateAreaExposedOverride;
    protected override double CalculateAreaExposed(Part part)
    {
        if (CalculateAreaExposedOverride == null)
        {
            return base.CalculateAreaExposed(part);
        }
        else
        {
            return CalculateAreaExposedOverride(part);
        }
    }

}
