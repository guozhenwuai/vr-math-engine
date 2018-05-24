using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MMaterial
{
    static Material defaultPointMat;

    static Material activePointMat;

    static Material selectPointMat;

    static Material specialPointMat;

    static Material defaultEdgeMat;

    static Material activeEdgeMat;

    static Material selectEdgeMat;

    static Material specialEdgeMat;

    static Material defaultFaceMat;

    static Material activeFaceMat;

    static Material selectFaceMat;

    static Material specialFaceMat;

    public static Material GetDefaultPointMat()
    {
        if(defaultPointMat == null)
        {
            defaultPointMat = Resources.Load<Material>("Materials/DefaultPointMat");
        }
        return defaultPointMat;
    }

    public static Material GetActivePointMat()
    {
        if (activePointMat == null)
        {
            activePointMat = Resources.Load<Material>("Materials/ActivePointMat");
        }
        return activePointMat;
    }

    public static Material GetSelectPointMat()
    {
        if (selectPointMat == null)
        {
            selectPointMat = Resources.Load<Material>("Materials/SelectPointMat");
        }
        return selectPointMat;
    }

    public static Material GetSpecialPointMat()
    {
        if(specialPointMat == null)
        {
            specialPointMat = Resources.Load<Material>("Materials/SpecialPointMat");
        }
        return specialPointMat;
    }

    public static Material GetDefaultEdgeMat()
    {
        if(defaultEdgeMat == null)
        {
			defaultEdgeMat = Resources.Load<Material>("Materials/DefaultEdgeMat");
        }
        return defaultEdgeMat;
    }

    public static Material GetActiveEdgeMat()
    {
        if (activeEdgeMat == null)
        {
            activeEdgeMat = Resources.Load<Material>("Materials/ActiveEdgeMat");
        }
        return activeEdgeMat;
    }

    public static Material GetSelectEdgeMat()
    {
        if (selectEdgeMat == null)
        {
            selectEdgeMat = Resources.Load<Material>("Materials/SelectEdgeMat");
        }
        return selectEdgeMat;
    }

    public static Material GetSpecialEdgeMat()
    {
        if (specialEdgeMat == null)
        {
            specialEdgeMat = Resources.Load<Material>("Materials/SpecialEdgeMat");
        }
        return specialEdgeMat;
    }

    public static Material GetDefaultFaceMat()
    {
        if (defaultFaceMat == null)
        {
            defaultFaceMat = Resources.Load<Material>("Materials/DefaultFaceMat");
        }
        return defaultFaceMat;
    }

    public static Material GetActiveFaceMat()
    {
        if (activeFaceMat == null)
        {
            activeFaceMat = Resources.Load<Material>("Materials/ActiveFaceMat");
        }
        return activeFaceMat;
    }

    public static Material GetSelectFaceMat()
    {
        if (selectFaceMat == null)
        {
            selectFaceMat = Resources.Load<Material>("Materials/SelectFaceMat");
        }
        return selectFaceMat;
    }

    public static Material GetSpecialFaceMat()
    {
        if (specialFaceMat == null)
        {
            specialFaceMat = Resources.Load<Material>("Materials/SpecialFaceMat");
        }
        return specialFaceMat;
    }
}