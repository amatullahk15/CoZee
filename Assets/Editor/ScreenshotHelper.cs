using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class ScreenshotHelper
{
    public static void TakeScreenshot()
    {
        MobileAppSceneBuilder.SetupAllScenes();

        // Ensure we are in the MainShell scene
        EditorSceneManager.OpenScene("Assets/Scenes/UI/MainShell.unity");

        // Give UI a frame to rebuild LayoutGroups (Canvas.ForceUpdateCanvases)
        Canvas.ForceUpdateCanvases();

        // Get the MainShellCanvas
        Canvas canvas = GameObject.Find("MainShellCanvas")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        Camera cam = new GameObject("ScreenshotCam").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic = true;
        
        // Match the 1080x2400 resolution
        int width = 1080;
        int height = 2400;
        cam.orthographicSize = height / 2f;
        cam.aspect = (float)width / height;

        // Position camera to see canvas if it's world space, but it's ScreenSpaceOverlay.
        // ScreenSpaceOverlay doesn't render to RenderTexture easily.
        // We will change it to ScreenSpaceCamera temporarily.
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;

        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        cam.Render();
        
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();
        
        byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes("e:/Unity/CoZee/screenshot.png", bytes);
        
        cam.targetTexture = null;
        RenderTexture.active = null; 
        GameObject.DestroyImmediate(rt);
        GameObject.DestroyImmediate(cam.gameObject);
        
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        Debug.Log("Screenshot saved to e:/Unity/CoZee/screenshot.png");
    }
}
