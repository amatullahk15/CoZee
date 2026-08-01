using UnityEngine;

public static class UserPreferences
{
    const string OnboardingDoneKey = "arfurniture_onboarding_done";
    const string PermissionsDoneKey = "arfurniture_permissions_done";
    const string LastTabKey = "arfurniture_last_tab";

    public static bool IsOnboardingDone
    {
        get => PlayerPrefs.GetInt(OnboardingDoneKey, 0) == 1;
        set => PlayerPrefs.SetInt(OnboardingDoneKey, value ? 1 : 0);
    }

    public static bool IsPermissionsDone
    {
        get => PlayerPrefs.GetInt(PermissionsDoneKey, 0) == 1;
        set => PlayerPrefs.SetInt(PermissionsDoneKey, value ? 1 : 0);
    }

    public static int LastTabIndex
    {
        get => PlayerPrefs.GetInt(LastTabKey, 0);
        set => PlayerPrefs.SetInt(LastTabKey, value);
    }

    public static void Save() => PlayerPrefs.Save();
}
