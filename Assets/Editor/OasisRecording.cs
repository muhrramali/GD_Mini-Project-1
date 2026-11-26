using System;
using System.IO;
using UnityEditor;
using UnityEditor.Media;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class OasisRecording
{
    private const int Width = 960;
    private const int Height = 540;
    private const int Fps = 24;
    private const int Total = 72 * Fps;
    private static Camera camera;
    private static Transform[] views;
    private static RenderTexture output;
    private static Texture2D image;
    private static MediaEncoder encoder;
    private static int frame;
    private static string directory;

    static OasisRecording()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode && File.Exists("RecordOasis.request"))
            {
                File.Delete("RecordOasis.request");
                Begin();
            }
            if (state == PlayModeStateChange.ExitingPlayMode && output != null) End();
        };
    }

    private static void Begin()
    {
        camera = Camera.main;
        if (camera == null || !camera.TryGetComponent<OasisExplorer>(out var explorer)) return;
        var points = new SerializedObject(explorer).FindProperty("viewpoints");
        views = new Transform[points.arraySize];
        for (int i = 0; i < views.Length; i++)
            views[i] = (Transform)points.GetArrayElementAtIndex(i).objectReferenceValue;
        directory = Path.GetFullPath("../tmp/oasis-final-capture");
        Directory.CreateDirectory(directory);
        output = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
        image = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
        frame = 0;
        Time.captureFramerate = Fps;
        EditorApplication.update += Capture;
    }

    private static void Capture()
    {
        try
        {
            if (frame < 4)
            {
                if (frame < 3) Pose(views[frame]);
                else camera.transform.SetPositionAndRotation(new Vector3(25, 11, -22),
                    Quaternion.LookRotation(new Vector3(-25, -7, 32)));
                Render();
                string[] names = { "01-Arrival", "02-Camp", "03-Shore", "04-Oasis" };
                File.WriteAllBytes(Path.Combine(directory, names[frame] + ".png"), image.EncodeToPNG());
                frame++;
                return;
            }
            if (encoder == null)
            {
                encoder = new MediaEncoder(Path.Combine(directory, "Caravan Stop Walkthrough.mp4"),
                    new VideoTrackAttributes { width = Width, height = Height, frameRate = new MediaRational(Fps) });
            }
            float seconds = (frame - 4) / (float)Fps;
            if (seconds < 8) Pose(views[0]);
            else if (seconds < 25) Move(views[0], views[1], (seconds - 8) / 17);
            else if (seconds < 34) Pose(views[1]);
            else if (seconds < 52)
            {
                float t = Mathf.SmoothStep(0, 1, (seconds - 34) / 18);
                var route = Vector3.Lerp(views[1].position, views[2].position, t);
                route.z -= 9 * Mathf.Sin(Mathf.PI * t);
                route.y += 1.8f * Mathf.Sin(Mathf.PI * t);
                camera.transform.SetPositionAndRotation(route, Quaternion.Slerp(views[1].rotation, views[2].rotation, t));
            }
            else if (seconds < 60) Pose(views[2]);
            else
            {
                float t = Mathf.SmoothStep(0, 1, (seconds - 60) / 12);
                var spot = Vector3.Lerp(views[2].position, new Vector3(-27, 11, 5), t);
                var angle = Quaternion.LookRotation(new Vector3(3, 10, 62) - spot);
                camera.transform.SetPositionAndRotation(spot, Quaternion.Slerp(views[2].rotation, angle, t));
            }
            Render();
            encoder.AddFrame(image);
            if (++frame >= Total + 4)
            {
                End();
                Debug.Log("Oasis walkthrough and four viewpoints saved.");
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            End();
        }
    }

    private static void Pose(Transform view) =>
        camera.transform.SetPositionAndRotation(view.position, view.rotation);

    private static void Move(Transform from, Transform to, float t)
    {
        t = Mathf.SmoothStep(0, 1, t);
        camera.transform.SetPositionAndRotation(Vector3.Lerp(from.position, to.position, t),
            Quaternion.Slerp(from.rotation, to.rotation, t));
    }

    private static void Render()
    {
        RenderPipeline.SubmitRenderRequest(camera, new RenderPipeline.StandardRequest { destination = output });
        var previous = RenderTexture.active;
        RenderTexture.active = output;
        try
        {
            image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            image.Apply();
        }
        finally { RenderTexture.active = previous; }
    }

    private static void End()
    {
        EditorApplication.update -= Capture;
        encoder?.Dispose();
        encoder = null;
        Time.captureFramerate = 0;
        UnityEngine.Object.DestroyImmediate(image);
        UnityEngine.Object.DestroyImmediate(output);
        output = null;
        Pose(views[0]);
    }
}

