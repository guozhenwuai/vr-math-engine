using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MMaterial
{
    static Material defaultPointMat;

    static Material activePointMat;

    static Material selectPointMat;

    static Material defaultEdgeMat;

    static Material activeEdgeMat;

    static Material selectEdgeMat;

    static Material defaultFaceMat;

    static Material activeFaceMat;

    static Material selectFaceMat;

    public static Material GetDefaultPointMat()
    {
        if(defaultPointMat == null)
        {
            defaultPointMat = Resources.Load<Material>("Materials/RedMat");
        }
        return defaultPointMat;
    }

    public static Material GetActivePointMat()
    {
        if (activePointMat == null)
        {
            activePointMat = Resources.Load<Material>("Materials/BlueMat");
        }
        return activePointMat;
    }

    public static Material GetSelectPointMat()
    {
        if (selectPointMat == null)
        {
            selectPointMat = Resources.Load<Material>("Materials/BlueMat");
        }
        return selectPointMat;
    }

    public static Material GetDefaultEdgeMat()
    {
        if(defaultEdgeMat == null)
        {
            defaultEdgeMat = Resources.Load<Material>("Materials/RedMat");
        }
        return defaultEdgeMat;
    }

    public static Material GetActiveEdgeMat()
    {
        if (activeEdgeMat == null)
        {
            activeEdgeMat = Resources.Load<Material>("Materials/BlueMat");
        }
        return activeEdgeMat;
    }

    public static Material GetSelectEdgeMat()
    {
        if (selectEdgeMat == null)
        {
            selectEdgeMat = Resources.Load<Material>("Materials/BlueMat");
        }
        return selectEdgeMat;
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
            selectFaceMat = Resources.Load<Material>("Materials/ActiveFaceMat");
        }
        return selectFaceMat;
    }
}