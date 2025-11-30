using UnityEngine;

public static class GlobalConfig
{
    private static SubDescriptionUIHelper subDescriptionUIHelper;

    public static SubDescriptionUIHelper SubDescriptionUIHelper
    {
        get
        {
            if (subDescriptionUIHelper == null)
            {
                subDescriptionUIHelper = Resources.Load<SubDescriptionUIHelper>("SubDescriptionUIHelper");
            }

            return subDescriptionUIHelper;
        }
    }
}