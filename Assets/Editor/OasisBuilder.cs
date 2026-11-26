using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class OasisBuilder
{
    private const string ScenePath = "Assets/Scenes/Caravan Stop.unity";
    private static Terrain terrain;

    [MenuItem("Oasis/Build Caravan Stop")]
    public static void Build()
    {

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        var sand = Mat("Dune sandstone", new Color(0.77f, 0.48f, 0.27f));
        var wood = Mat("Acacia wood", new Color(0.29f, 0.13f, 0.07f));
        var teal = Mat("Dyed teal canvas", new Color(0.05f, 0.35f, 0.32f));
        var red = Mat("Terracotta canvas", new Color(0.64f, 0.19f, 0.12f));
        var cream = Mat("Woven linen", new Color(0.87f, 0.67f, 0.38f));
        var leaf = Mat("Palm green", new Color(0.24f, 0.42f, 0.13f));
        var water = Mat("Oasis water", new Color(0.025f, 0.38f, 0.43f));
        water.SetFloat("_Smoothness", 0.82f);
        var glow = Mat("Amber lantern glass", new Color(0.95f, 0.44f, 0.08f));
        glow.EnableKeyword("_EMISSION");
        glow.SetColor("_EmissionColor", new Color(2.5f, 0.9f, 0.16f));

        BuildTerrain();
        var landscape = Group("Oasis and palms");
        Shape("Water", PrimitiveType.Cylinder, new Vector3(0, 2.05f, 4), new Vector3(48, 0.025f, 36), water, landscape);
        Random.InitState(237);
        for (int i = 0; i < 29; i++)
        {
            float angle = i * Mathf.PI * 2 / 29f;
            float x = Mathf.Cos(angle) * Random.Range(19f, 26f);
            float z = 4 + Mathf.Sin(angle) * Random.Range(13.8f, 20f);
            if (z < 0 && x < 11 && x > -8) continue;
            Palm(x, z, Random.Range(3.2f, 6.9f), wood, leaf, landscape);
        }

        var camp = Group("Caravan camp");
        Tent(-10, -18, -18, teal, cream, wood, glow, camp);
        Tent(1, -20, 9, red, cream, wood, glow, camp);
        Tent(13, -15, 24, cream, red, wood, glow, camp);
        for (int i = 0; i < 16; i++)
        {
            float x = Random.Range(-13f, 15f), z = Random.Range(-25f, -23f);
            float size = Random.Range(0.45f, 0.9f);
            var crate = Shape("Cargo crate", PrimitiveType.Cube, Ground(x, z, size / 2),
                Vector3.one * size, i % 3 == 0 ? sand : wood, camp);
            crate.transform.Rotate(0, Random.Range(0, 90), 0);
        }
        var banner = Group("Swaying caravan banner", camp);
        banner.position = Ground(-6, -20, 3.1f);
        Shape("Banner pole", PrimitiveType.Cylinder, banner.position - Vector3.up * 1.55f,
            new Vector3(.12f, 1.55f, .12f), wood, camp);
        Shape("Banner cloth", PrimitiveType.Cube, banner.position + new Vector3(.45f, -.32f, 0),
            new Vector3(.9f, .63f, .035f), red, banner);
        banner.gameObject.AddComponent<OasisSway>();
        var cart = Group("Supply cart", camp);
        cart.position = Ground(10, -22);
        Shape("Cart bed", PrimitiveType.Cube, cart.position + Vector3.up * 0.9f,
            new Vector3(1.8f, 0.28f, 2.8f), wood, cart);
        foreach (float side in new[] { -1f, 1f })
        {
            var wheel = Shape("Cart wheel", PrimitiveType.Cylinder, cart.position + new Vector3(side, 0.64f, 0),
                new Vector3(1.3f, 0.12f, 1.3f), sand, cart);
            wheel.transform.Rotate(0, 0, 90);
            Shape("Cart rail", PrimitiveType.Cube, cart.position + new Vector3(side * .8f, 1.2f, 0),
                new Vector3(.16f, .5f, 2.8f), wood, cart);
            Shape("Cart handle", PrimitiveType.Cube, cart.position + new Vector3(side * .55f, .85f, -2),
                new Vector3(.12f, .12f, 2.3f), wood, cart);
        }

        var ruins = Group("Sandstone gateway");
        for (int side = -1; side <= 1; side += 2)
        {
            var foot = Ground(side * 3.3f + 3, 62);
            for (int j = 0; j < 6; j++)
            {
                var block = Shape("Weathered arch block", PrimitiveType.Cube, foot + Vector3.up * (j * 1.4f + .7f),
                    new Vector3(2.5f - j * .09f, 1.5f, 2.3f), sand, ruins);
                block.transform.Rotate(0, Random.Range(-8f, 8f), side * j * 1.5f);
            }
        }
        var center = Ground(3, 62);
        Shape("Gateway lintel", PrimitiveType.Cube, center + Vector3.up * 8.65f,
            new Vector3(9.6f, 1.55f, 2.5f), sand, ruins);
        Shape("Gateway cap", PrimitiveType.Cube, center + Vector3.up * 9.6f,
            new Vector3(10.3f, .55f, 2.85f), sand, ruins);
        for (int i = 0; i < 65; i++)
        {
            float x = Random.Range(-105f, 105f), z = Random.Range(-110f, 115f);
            if (Mathf.Abs(x) < 30 && z > -48 && z < 58) continue;
            var rock = Shape("Sandstone outcrop", PrimitiveType.Cube, Ground(x, z), new Vector3(
                Random.Range(1f, 6f), Random.Range(1f, 5f), Random.Range(1f, 6f)), sand, ruins);
            rock.transform.rotation = Quaternion.Euler(Random.Range(-20f, 20f), Random.Range(0, 360), Random.Range(-30f, 30f));
        }

        var lighting = Group("Lighting");
        var sun = new GameObject("High desert sun", typeof(Light)).GetComponent<Light>();
        sun.transform.SetParent(lighting);
        sun.transform.rotation = Quaternion.Euler(54, -36, 0);
        sun.type = LightType.Directional;
        sun.color = new Color(1, .87f, .69f);
        sun.intensity = 1.65f;
        sun.shadows = LightShadows.Soft;
        var sky = new Material(Shader.Find("Skybox/Procedural")) { name = "Desert haze sky" };
        sky.SetColor("_SkyTint", new Color(.55f, .69f, .75f));
        sky.SetColor("_GroundColor", new Color(.73f, .53f, .36f));
        sky.SetFloat("_AtmosphereThickness", .93f);
        Save(sky, "Desert haze sky.mat");
        RenderSettings.skybox = sky;
        RenderSettings.sun = sun;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(.81f, .71f, .57f);
        RenderSettings.fogDensity = .008f;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.61f, .64f, .69f);

        var views = Group("Viewpoints");
        var first = View("1 Arrival ridge", Ground(-1, -45, 9), new Vector3(0, 5, 12), views);
        var second = View("2 Caravan camp", Ground(15, -27, 3), new Vector3(1, 3, -16), views);
        var third = View("3 Oasis shore", Ground(-19, -2, 2.6f), new Vector3(7, 4, 19), views);
        var camera = new GameObject("Explorer camera", typeof(Camera), typeof(OasisExplorer));
        camera.tag = "MainCamera";
        camera.transform.SetPositionAndRotation(first.position, first.rotation);
        camera.GetComponent<Camera>().fieldOfView = 62;
        var settings = new SerializedObject(camera.GetComponent<OasisExplorer>());
        settings.FindProperty("ground").objectReferenceValue = terrain;
        settings.FindProperty("flightPlayerPrefab").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scenes/shadow.prefab");
        settings.FindProperty("flightPlayerMaterial").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Dyed teal canvas.mat");
        var points = settings.FindProperty("viewpoints");
        points.arraySize = 3;
        points.GetArrayElementAtIndex(0).objectReferenceValue = first;
        points.GetArrayElementAtIndex(1).objectReferenceValue = second;
        points.GetArrayElementAtIndex(2).objectReferenceValue = third;
        settings.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.SaveAssets();
        const string path = ScenePath;
        if (!EditorSceneManager.SaveScene(scene, path)) throw new System.Exception("Could not save oasis scene.");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
        Debug.Log("Oasis scene saved: " + path);
    }

    private static void BuildTerrain()
    {
        const int resolution = 129;
        var data = new TerrainData
        {
            heightmapResolution = resolution, alphamapResolution = 128, size = new Vector3(240, 36, 240)
        };
        var heights = new float[resolution, resolution];
        for (int z = 0; z < resolution; z++)
        for (int x = 0; x < resolution; x++)
        {
            float wx = x * 240f / (resolution - 1) - 120;
            float wz = z * 240f / (resolution - 1) - 120;
            float basin = Mathf.Sqrt(Mathf.Pow(wx / 22, 2) + Mathf.Pow((wz - 4) / 16, 2));
            float rise = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.78f, 1.42f, basin));
            float outer = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(29, 102, new Vector2(wx, wz).magnitude));
            float dune = Mathf.Pow(.5f + .5f * Mathf.Sin(wx * .092f + wz * .062f +
                Mathf.Sin(wz * .048f) * 1.8f), 2);
            float edge = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(87, 117, Mathf.Max(Mathf.Abs(wx), Mathf.Abs(wz))));
            heights[z, x] = (.8f + rise * (2.5f + outer * (2 + dune * 10)) + edge * 13) / 36;
        }
        data.SetHeights(0, 0, heights);
        Save(data, "Caravan dunes.asset");
        data.terrainLayers = new[]
        {
            Layer("Dry sand", new Color(.82f, .56f, .32f)),
            Layer("Damp sand", new Color(.52f, .35f, .19f)),
            Layer("Rocky sand", new Color(.59f, .36f, .22f))
        };
        PaintTerrain(data);
        var ground = Terrain.CreateTerrainGameObject(data);
        ground.name = "Terrain";
        ground.transform.position = new Vector3(-120, 0, -120);
        terrain = ground.GetComponent<Terrain>();
    }

    public static void PaintTerrain(TerrainData data)
    {
        var weights = new float[128, 128, 3];
        for (int z = 0; z < 128; z++)
        for (int x = 0; x < 128; x++)
        {
            float u = x / 127f, v = z / 127f;
            float h = data.GetInterpolatedHeight(u, v);
            float wet = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(1.75f, 3.6f, h));
            float rock = Mathf.SmoothStep(0, .72f, Mathf.InverseLerp(5, 28, data.GetSteepness(u, v))) * (1 - wet);
            weights[z, x, 0] = 1 - wet - rock;
            weights[z, x, 1] = wet;
            weights[z, x, 2] = rock;
        }
        data.SetAlphamaps(0, 0, weights);
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
    }

    private static TerrainLayer Layer(string label, Color color)
    {
        var texture = new Texture2D(64, 64);
        var pixels = new Color[64 * 64];
        for (int y = 0; y < 64; y++)
        for (int x = 0; x < 64; x++)
        {
            float grain = .91f + Mathf.PerlinNoise(x * .32f, y * .32f) * .16f;
            pixels[y * 64 + x] = color * grain;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        Save(texture, label + " texture.asset");
        var layer = new TerrainLayer { diffuseTexture = texture, tileSize = new Vector2(5, 5) };
        Save(layer, label + ".terrainlayer");
        return layer;
    }

    private static void Tent(float x, float z, float yaw, Material canvas, Material trim, Material wood, Material glow, Transform parent)
    {
        var tent = Group("Caravan tent", parent);
        tent.position = Ground(x, z);
        tent.rotation = Quaternion.Euler(0, yaw, 0);
        foreach (float side in new[] { -1f, 1f })
        {
            var roof = Shape("Canvas roof", PrimitiveType.Cube, tent.TransformPoint(side * 1.24f, 2.16f, 0),
                new Vector3(2.95f, .09f, 5), canvas, tent);
            roof.transform.localRotation = Quaternion.Euler(0, 0, side * -27);
            foreach (float end in new[] { -1f, 1f })
                Shape("Tent pole", PrimitiveType.Cylinder, tent.TransformPoint(side * 2.4f, .77f, end * 2.2f),
                    new Vector3(.14f, .86f, .14f), wood, tent);
        }
        Shape("Woven rug", PrimitiveType.Cube, tent.TransformPoint(0, .10f, -.3f), new Vector3(3.9f, .06f, 4.6f), trim, tent);
        Shape("Lantern", PrimitiveType.Sphere, tent.TransformPoint(0, 2.14f, -2.1f), Vector3.one * .28f, glow, tent);
        var light = new GameObject("Tent lantern light", typeof(Light)).GetComponent<Light>();
        light.transform.SetParent(tent, false);
        light.transform.localPosition = new Vector3(0, 2.03f, -2.05f);
        light.type = LightType.Point;
        light.range = 5;
        light.intensity = 2.4f;
        light.color = new Color(1, .65f, .32f);
    }

    private static void Palm(float x, float z, float height, Material wood, Material leaf, Transform parent)
    {
        var foot = Ground(x, z);
        Shape("Palm trunk", PrimitiveType.Cylinder, foot + Vector3.up * (height / 2),
            new Vector3(.49f, height / 2, .49f), wood, parent);
        var crown = Group("Swaying palm crown", parent);
        crown.position = foot + Vector3.up * height;
        var sway = new SerializedObject(crown.gameObject.AddComponent<OasisSway>());
        sway.FindProperty("phase").floatValue = Random.value;
        sway.ApplyModifiedPropertiesWithoutUndo();
        for (int j = 0; j < 8; j++)
        {
            float angle = j * 45 + Random.Range(-12, 12);
            var direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            var frond = Shape("Palm frond", PrimitiveType.Sphere, crown.position + direction * 1.12f,
                new Vector3(.66f, .17f, 2.65f), leaf, crown);
            frond.transform.rotation = Quaternion.Euler(Random.Range(7, 22), angle, 0);
        }
    }

    private static Material Mat(string label, Color color)
    {
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = label };
        material.SetColor("_BaseColor", color);
        Save(material, label + ".mat");
        return material;
    }

    private static void Save(Object asset, string filename)
    {
        string folder = asset is Material ? "Materials" : asset is Texture2D ? "Textures" : "Terrain";
        AssetDatabase.CreateAsset(asset, "Assets/Art/" + folder + "/" + filename);
    }

    private static Vector3 Ground(float x, float z, float offset = 0) =>
        new Vector3(x, terrain.SampleHeight(new Vector3(x, 0, z)) + offset, z);

    private static Transform Group(string label, Transform parent = null)
    {
        var group = new GameObject(label).transform;
        group.SetParent(parent, false);
        return group;
    }

    private static GameObject Shape(string label, PrimitiveType primitive, Vector3 position, Vector3 size, Material material, Transform parent)
    {
        var item = GameObject.CreatePrimitive(primitive);
        item.name = label;
        item.transform.SetParent(parent, false);
        item.transform.position = position;
        item.transform.localScale = size;
        item.GetComponent<Renderer>().sharedMaterial = material;
        return item;
    }

    private static Transform View(string label, Vector3 position, Vector3 target, Transform parent)
    {
        var point = Group(label, parent);
        point.SetPositionAndRotation(position, Quaternion.LookRotation(target - position));
        return point;
    }
}





