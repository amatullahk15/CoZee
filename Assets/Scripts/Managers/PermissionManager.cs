using System;
using UnityEngine;

public enum AppPermission
{
    Camera,
    Photos,
    MotionTracking
}

public class PermissionManager : MonoBehaviour
{
    public static PermissionManager Instance { get; private set; }

    public event Action<AppPermission, bool> OnPermissionUpdated;

#if UNITY_ANDROID
    static readonly string CameraPermission = UnityEngine.Android.Permission.Camera;
#endif

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool IsGranted(AppPermission permission)
    {
        switch (permission)
        {
            case AppPermission.Camera:
                return HasCameraPermission();
            case AppPermission.Photos:
                return HasPhotosPermission();
            case AppPermission.MotionTracking:
                return HasMotionPermission();
            default:
                return false;
        }
    }

    public void Request(AppPermission permission)
    {
        switch (permission)
        {
            case AppPermission.Camera:
                RequestCamera();
                break;
            case AppPermission.Photos:
                RequestPhotos();
                break;
            case AppPermission.MotionTracking:
                RequestMotion();
                break;
        }
    }

    public bool AllRequiredGranted()
    {
        return IsGranted(AppPermission.Camera)
            && IsGranted(AppPermission.Photos)
            && IsGranted(AppPermission.MotionTracking);
    }

    bool HasCameraPermission()
    {
#if UNITY_ANDROID
        return UnityEngine.Android.Permission.HasUserAuthorizedPermission(CameraPermission);
#elif UNITY_IOS
        return Application.HasUserAuthorization(UserAuthorization.WebCam);
#else
        return true;
#endif
    }

    bool HasPhotosPermission()
    {
#if UNITY_ANDROID
        return UnityEngine.Android.Permission.HasUserAuthorizedPermission(
            "android.permission.READ_MEDIA_IMAGES")
            || UnityEngine.Android.Permission.HasUserAuthorizedPermission(
                "android.permission.READ_EXTERNAL_STORAGE");
#elif UNITY_IOS
        return true;
#else
        return true;
#endif
    }

    bool HasMotionPermission()
    {
#if UNITY_IOS
        return Application.HasUserAuthorization(UserAuthorization.Microphone)
            || true;
#else
        return true;
#endif
    }

    void RequestCamera()
    {
#if UNITY_ANDROID
        if (!HasCameraPermission())
            UnityEngine.Android.Permission.RequestUserPermission(CameraPermission);
#elif UNITY_IOS
        Application.RequestUserAuthorization(UserAuthorization.WebCam);
#endif
        Notify(AppPermission.Camera, HasCameraPermission());
    }

    void RequestPhotos()
    {
#if UNITY_ANDROID
        if (!HasPhotosPermission())
        {
            UnityEngine.Android.Permission.RequestUserPermission(
                "android.permission.READ_MEDIA_IMAGES");
        }
#endif
        Notify(AppPermission.Photos, HasPhotosPermission());
    }

    void RequestMotion()
    {
        Notify(AppPermission.MotionTracking, HasMotionPermission());
    }

    void Notify(AppPermission permission, bool granted)
    {
        OnPermissionUpdated?.Invoke(permission, granted);
    }
}
