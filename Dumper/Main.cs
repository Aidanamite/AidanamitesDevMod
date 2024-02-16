using BepInEx;
using ConfigTweaks;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AidanamitesDevMod
{
    [BepInPlugin("com.aidanamite.AidanamitesDevMod", "AidanamitesDevMod", "1.1.0")]
    [BepInDependency("com.aidanamite.ConfigTweaks")]
    public class Main : BaseUnityPlugin
    {
        public void Awake()
        {
            Logger.LogInfo("Loaded");
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad0) && SanctuaryManager.pCurPetInstance)
            {
                Logger.LogInfo("Starting render resource load");
                var l = new Dictionary<UserItemData, GameObject>();
                foreach (var i in CommonInventoryData.pInstance.GetItems(Category.DragonSkin))
                    if (!string.IsNullOrEmpty(i?.Item?.AssetName) && i.Item.ItemID >= 100000 && i.Item.GetAttribute("PetTypeID", -1) == SanctuaryManager.pCurPetInstance.pData.PetTypeID && !l.ContainsKey(i))
                        l[i] = RsResourceManager.LoadAssetFromBundle(i.Item.AssetName, typeof(GameObject)) as GameObject;
                {
                    var t = RenderingTools.RenderImages(SanctuaryManager.pCurPetInstance.gameObject, l.Where(x => x.Value).Select(x =>
                        new RenderingTools.RenderConfig(1024, 1024, Camera.main.transform.rotation)
                        {
                            ambientLight = Color.white,
                            complexTransparencyDetection = true,
                            orthographic = false,
                            renderHeightOverride = 50,
                            BeforeRender = (y) =>
                            {
                                Logger.LogInfo($"Attempting to render {x.Key.Item.ItemName} >> {x.Value}");
                                y.GetComponent<SanctuaryPet>().SetAccessory(RaisedPetAccType.Materials, Instantiate(x.Value), null);
                                foreach (var i in y.GetComponentsInChildren<Renderer>(true))
                                    if (i)
                                    {
                                        if (i.name == "RotationPivot" || i is TrailRenderer)
                                        {
                                            Logger.LogInfo($"Destroying: {i} - {i.bounds}");
                                            if (i.name == "RotationPivot")
                                                DestroyImmediate(i.gameObject);
                                            else
                                                DestroyImmediate(i);
                                        }
                                        else
                                        {
                                            if (i is SkinnedMeshRenderer skin)
                                            {
                                                if (!skin.sharedMesh || !skin.enabled)
                                                {
                                                    skin.gameObject.SetActive(false);
                                                    Logger.LogInfo($"Hiding: {i} - {i.bounds}");
                                                }
                                                else
                                                {
                                                    skin.updateWhenOffscreen = true;
                                                    Logger.LogInfo($"Keeping: {i} - {i.bounds}");
                                                }
                                            }
                                            else
                                                Logger.LogInfo($"Keeping: {i} - {i.bounds}");
                                        }
                                    }
                            }
                        }).ToArray(),
                        out var e, lightingMode: RenderingTools.LightingMode.IgnoreAllLights, cameraBufferDistance: 1, renderLayer: 31);
                    for (int i = 0; i < t.Length; i++)
                        if (t[i])
                        {
                            if (!Directory.Exists("Renders"))
                                Directory.CreateDirectory("Renders");
                            File.WriteAllBytes($"Renders/Img-{DateTime.UtcNow.Ticks}.png", t[i].EncodeToPNG());
                        }
                        else
                        {
                            Logger.LogError($"Failed to render image. Reason: {e[i]?.ToString() ?? "Unknown"}");
                        }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Keypad1) && SanctuaryManager.pCurPetInstance)
            {
                Logger.LogInfo("Starting render resource load");
                var l = new Dictionary<UserItemData,(GameObject,Texture2D)>();
                foreach (var i in CommonInventoryData.pInstance.GetItems(Category.DragonSaddles))
                    if (!string.IsNullOrEmpty(i?.Item?.AssetName) && i.Item.Texture?.Length >= 1 && i.Item.ItemID >= 100000 && i.Item.GetAttribute("PetTypeID", -1) == SanctuaryManager.pCurPetInstance.pData.PetTypeID && !l.ContainsKey(i))
                        l.Add(i,(RsResourceManager.LoadAssetFromBundle(i.Item.AssetName, typeof(GameObject)) as GameObject, RsResourceManager.LoadAssetFromBundle(i.Item.Texture[0].TextureName, typeof(Texture2D)) as Texture2D));
                {
                    var t = RenderingTools.RenderImages(SanctuaryManager.pCurPetInstance.gameObject, l.Where(x => x.Value.Item1 && x.Value.Item2).Select(x =>
                        new RenderingTools.RenderConfig(1024, 1024, Camera.main.transform.rotation)
                        {
                            ambientLight = Color.white,
                            complexTransparencyDetection = true,
                            orthographic = false,
                            renderHeightOverride = 50,
                            BeforeRender = (y) =>
                            {
                                Logger.LogInfo($"Attempting to render {x.Key.Item.ItemName} >> {x.Value.Item1}, {x.Value.Item2}");
                                y.GetComponent<SanctuaryPet>().SetAccessory(RaisedPetAccType.Saddle, Instantiate( x.Value.Item1), Instantiate(x.Value.Item2));
                                foreach (var i in y.GetComponentsInChildren<Renderer>(true))
                                    if (i)
                                    {
                                        if (!i.name.Contains("Saddle"))
                                        {
                                            Logger.LogInfo($"Hiding: {i} - {i.bounds}");
                                            i.gameObject.SetActive(false);
                                        }
                                        else
                                        {
                                            if (i is SkinnedMeshRenderer skin)
                                                    skin.updateWhenOffscreen = true;
                                                Logger.LogInfo($"Keeping: {i} - {i.bounds}");
                                        }
                                    }
                            }
                        }).ToArray(),
                        out var e, lightingMode: RenderingTools.LightingMode.IgnoreAllLights, cameraBufferDistance: 1, renderLayer: 31);
                    for (int i = 0; i < t.Length; i++)
                        if (t[i])
                        {
                            if (!Directory.Exists("Renders"))
                                Directory.CreateDirectory("Renders");
                            File.WriteAllBytes($"Renders/Img-{DateTime.UtcNow.Ticks}.png", t[i].EncodeToPNG());
                        }
                        else
                        {
                            Logger.LogError($"Failed to render image. Reason: {e[i]?.ToString() ?? "Unknown"}");
                        }
                }
            }
        }
    }

    public static class RenderingTools
    {


        /// <summary>
        /// Lighting options for the object rendering methods:<br/>
        /// <see cref="RenderImage(GameObject, RenderConfig, out Exception, bool, LightingMode, int, float)"/><br/>
        /// <see cref="RenderImages(GameObject, RenderConfig[], out Exception[], bool, LightingMode, int, float)"/>
        /// </summary>
        public enum LightingMode
        {
            /// <summary>Make no changes to the light sources for renders</summary>
            DoNotChange,
            /// <summary>Only include light sources part of the object itself for renders</summary>
            IgnoreExternalLights,
            /// <summary>Don't include any light sources in the renders</summary>
            IgnoreAllLights
        }

        /// <summary>
        /// Image generation settings for the object rendering methods:<br/>
        /// <see cref="RenderImage(GameObject, RenderConfig, out Exception, bool, LightingMode, int, float)"/><br/>
        /// <see cref="RenderImages(GameObject, RenderConfig[], out Exception[], bool, LightingMode, int, float)"/>
        /// </summary>
        public class RenderConfig
        {
            /// <summary>Action to be run before starting this render</summary>
            public Action<GameObject> BeforeRender;
            /// <summary>Action to be run after this render. Use for undoing changes made in the before action</summary>
            public Action<GameObject> AfterRender;
            /// <summary>Ambient lighting to use for this render. Leave <see langword="null"/> to use current</summary>
            public Color? ambientLight;
            /// <summary>Background color to use for the render. Transparent colors can be used</summary>
            public Color backgroundColor = Color.clear;
            /// <summary>The angle the camera should point during the render. Object will be centered in the image regardless</summary>
            public Quaternion cameraAngle;
            /// <summary>The margins of transparent pixels to ensure are around the rendered image. Set to <see langword="null"/> to skip margin checks</summary>
            public (uint left, uint bottom, uint right, uint top)? imageMargins = (0, 0, 0, 0);
            /// <summary>Width in pixels of the image to be created</summary>
            public uint imgWidth;
            /// <summary>Height in pixels of the image to be created</summary>
            public uint imgHeight;
            /// <summary>Mipmap count of the image to be created</summary>
            public int mipmapCount = 0;
            /// <summary>Whether to use the linear color space or the sRGB color space for the image to be created</summary>
            public bool linearImg = QualitySettings.activeColorSpace == ColorSpace.Linear;
            /// <summary>Not recommended unless you're using an object that is only particles or the particles you want to be included are not already included</summary>
            public bool respectParticleRendererBounds = false;
            /// <summary>Whether to use a more detailed transparency detection. This will do more processing but will help if you're using a transparent background and are having issues with gamma artifacting in the render. This will have no effect if you're not using margins</summary>
            public bool complexTransparencyDetection = false;
            /// <summary>If <see langword="null"/>, render height will be automatically calculated using object bounds, and will be adjusted to better fit margins if provided. Otherwise, the "height" of the area to be rendered</summary>
            public float? renderHeightOverride;
            /// <summary>Whether or not to use an orthographic camera for the render</summary>
            public bool orthographic = true;
            /// <summary>If <see langword="null"/>, object center will be automatically calculated using object bounds, and will be adjusted to better fit margins if provided. Otherwise, the world-space point that is the center of the area to be rendered</summary>
            public Vector3? centerOverride;
            /// <summary>Sets the <see cref="DepthTextureMode"/> for the render to use. Leave <see langword="null"/> to use the current main camera's setting</summary>
            public DepthTextureMode? depthModeOverride;
            /// <summary>Sets whether the rendering camera should allow HDR for the render. Leave <see langword="null"/> to use the current main camera's setting</summary>
            public bool? allowHDROverride;
            /// <summary>Sets whether the rendering camera should allow HDR for the render. Leave <see langword="null"/> to use the current main camera's setting</summary>
            public bool? allowMSAAOverride;
            /// <summary>Sets the render camera's depth for the render. Leave <see langword="null"/> to use the current main camera's setting</summary>
            public float? cameraDepthOverride;
            /// <summary>Sets the <see cref="Camera.GateFitMode"/> for the render to use. Leave <see langword="null"/> to use the current main camera's setting</summary>
            public Camera.GateFitMode? gateFitOverride;
            /// <summary>
            /// Image generation settings for the object rendering methods:<br/>
            /// <see cref="RenderImage(GameObject, RenderConfig, out Exception, bool, LightingMode, int, float)"/><br/>
            /// <see cref="RenderImages(GameObject, RenderConfig[], out Exception[], bool, LightingMode, int, float)"/>
            /// </summary>
            public RenderConfig(uint width, uint height, Quaternion angle)
            {
                imgWidth = width;
                imgHeight = height;
                cameraAngle = angle;
            }
        }

        static Texture2D RenderImage(GameObject go, RenderConfig renderConfig, out Exception exception, bool copyObjectForRender, LightingMode lightingMode, int renderLayer) => go.RenderImage(renderConfig, out exception, copyObjectForRender, lightingMode, renderLayer); // Preventing MissingMethodException
        static Texture2D[] RenderImages(GameObject go, RenderConfig[] renderConfigs, out Exception[] exceptions, bool copyObjectForRender, LightingMode lightingMode, int renderLayer) => go.RenderImages(renderConfigs, out exceptions, copyObjectForRender, lightingMode, renderLayer); // Preventing MissingMethodException
        
        /// <summary>
        /// Renders an image of the object using the provided configuration.
        /// </summary>
        /// <param name="copyObjectForRender">if <see langword="true"/>, it will instantiate the object and render the instantiated copy, otherwise will render the object itself. This will almost always need to be enabled for rendering a prefab</param>
        /// <param name="exception">Will be set to whatever <see cref="Exception"/> occured while trying to render, if one occured. Otherwise will be <see langword="null"/></param>
        /// <param name="go">The object to render an image of</param>
        /// <param name="lightingMode">Which light sources should be included in the render</param>
        /// <param name="renderConfig">The configuration to use for rendering the image</param>
        /// <param name="renderLayer">The layer to use for the render. It is recommended to leave as 30 to avoid unwanted object being included in the image</param>
        /// <param name="cameraBufferDistance">The amount of unrendered distance between the camera and the object. You shouldn't need to increase this unless you're getting strange object artifacting in the render</param>
        /// <returns><see langword="null"/> and sets the <paramref name="exception"/> value if an <see cref="Exception"/> occured, otherwise the generated image</returns>
        public static Texture2D RenderImage(this GameObject go, RenderConfig renderConfig, out Exception exception, bool copyObjectForRender = true, LightingMode lightingMode = LightingMode.DoNotChange, int renderLayer = 30, float cameraBufferDistance = 10)
        {
            var r = go.RenderImages(new[] { renderConfig }, out var exceptions, copyObjectForRender, lightingMode, renderLayer, cameraBufferDistance)[0];
            exception = exceptions[0];
            return r;
        }
        /// <summary>
        /// Renders multiple images of the object using the provided configurations.
        /// </summary>
        /// <param name="copyObjectForRender">if <see langword="true"/>, it will instantiate the object and render the instantiated copy, otherwise will render the object itself. This will almost always need to be enabled for rendering a prefab</param>
        /// <param name="exceptions">Will be set to an array of the same size as <paramref name="renderConfigs"/>. For any renders that fail, the error that occured will be stored at the respective index</param>
        /// <param name="go">The object to render an image of</param>
        /// <param name="lightingMode">Which light sources should be included in the render</param>
        /// <param name="renderConfigs">The configurations to use for rendering the images</param>
        /// <param name="renderLayer">The layer to use for the renders. It is recommended to leave as 30 to avoid unwanted object being included in the images</param>
        /// <param name="cameraBufferDistance">The amount of unrendered distance between the camera and the object. You shouldn't need to increase this unless you're getting strange object artifacting in the renders</param>
        /// <returns>An array of the same size as <paramref name="renderConfigs"/> containing the images generated, respective of the indecies. If an error occured while generating a particular image, a <see langword="null"/> will be stored at that index and the exception will be stored in <paramref name="exceptions"/> at said index</returns>
        public static Texture2D[] RenderImages(this GameObject go, RenderConfig[] renderConfigs, out Exception[] exceptions, bool copyObjectForRender = true, LightingMode lightingMode = LightingMode.DoNotChange, int renderLayer = 30, float cameraBufferDistance = 10)
        {
            var results = new Texture2D[renderConfigs.Length];
            exceptions = new Exception[renderConfigs.Length];
            if (renderConfigs.Length == 0)
                return results;
            var created = new List<Object>();
            var originalTimeScale = Time.timeScale;
            var originalFixedTimeScale = Time.fixedDeltaTime;
            Time.timeScale = 0;
            Time.fixedDeltaTime = 0;
            var originalLights = new List<Light>();
            var originalSkybox = RenderSettings.skybox;
            var originalLight = RenderSettings.ambientLight;
            var originalFog = RenderSettings.fog;
            RenderSettings.fog = false;
            var originalLayers = new Dictionary<GameObject, int>();
            var originalForceRenderingOff = new HashSet<Renderer>();
            try
            {
                var o = go;
                var keepLights = new HashSet<Light>();
                if (copyObjectForRender)
                {
                    var tmp = new GameObject("");
                    created.Add(tmp);
                    tmp.SetActive(false);
                    var oName = o.name;
                    o = Object.Instantiate(o,tmp.transform);
                    created.Add(o);
                    o.name = oName;
                    o.transform.SetParent(null);
                    Object.Destroy(tmp);
                }
                else
                    foreach (var t in o.GetComponentsInChildren<Transform>(true))
                        originalLayers[t.gameObject] = t.gameObject.layer;
                if (lightingMode == LightingMode.IgnoreExternalLights)
                    foreach (var l in o.GetComponentsInChildren<Light>())
                        keepLights.Add(l);
                if (lightingMode >= LightingMode.IgnoreExternalLights)
                {
                    foreach (var l in Object.FindObjectsOfType<Light>())
                        if (!keepLights.Contains(l) && l.enabled)
                        {
                            originalLights.Add(l);
                            l.enabled = false;
                        }
                    RenderSettings.skybox = null;
                }
                var c = new GameObject("").AddComponent<Camera>();
                created.Add(c.gameObject);
                c.cullingMask = 1 << renderLayer;
                c.clearFlags = CameraClearFlags.SolidColor;
                c.nearClipPlane = cameraBufferDistance;
                c.useOcclusionCulling = false;
                var depthTextureMode = Camera.main ? DepthTextureMode.Depth | DepthTextureMode.DepthNormals : Camera.main.depthTextureMode;
                var allowHDR = Camera.main ? true : Camera.main.allowHDR;
                var allowMSAA = Camera.main ? false : Camera.main.allowMSAA;
                var depth = Camera.main ? c.depth : Camera.main.depth;
                var gateFit = Camera.main ? Camera.GateFitMode.Horizontal : Camera.main.gateFit;
                var ind = 0;
                foreach (var config in renderConfigs)
                {
                    try
                    {
                        config.BeforeRender?.Invoke(o);
                        c.orthographic = config.orthographic;
                        if (config.imgHeight == 0 || config.imgWidth == 0)
                            throw new InvalidOperationException($"Image is too small [Width={config.imgWidth},Height={config.imgHeight}]");
                        var innerWidth = (int)(config.imageMargins != null ? config.imgWidth - config.imageMargins.Value.right - config.imageMargins.Value.left : config.imgWidth);
                        var innerHeight = (int)(config.imageMargins != null ? config.imgHeight - config.imageMargins.Value.top - config.imageMargins.Value.bottom : config.imgHeight);
                        if (innerHeight <= 0 || innerWidth <= 0)
                            throw new InvalidOperationException($"Image is too small [Width={config.imgWidth},Height={config.imgHeight},WidthWithoutMargins={innerWidth},HeightWithoutMargins={innerHeight}]");
                        var min = Vector3.positiveInfinity;
                        var max = Vector3.negativeInfinity;
                        var count = 0;
                        foreach (var rend in o.GetComponentsInChildren<Renderer>())
                        {
                            if (!originalForceRenderingOff.Contains(rend) && rend.forceRenderingOff)
                            {
                                rend.forceRenderingOff = false;
                                rend.enabled = true;
                                originalForceRenderingOff.Add(rend);
                            }
                            rend.gameObject.layer = renderLayer;
                            if (config.respectParticleRendererBounds || !(rend is ParticleSystemRenderer))
                            {
                                count++;
                                var b = rend.bounds;
                                var v = b.min;
                                min.x = Math.Min(min.x, v.x);
                                min.y = Math.Min(min.y, v.y);
                                min.z = Math.Min(min.z, v.z);
                                v = b.max;
                                max.x = Math.Max(max.x, v.x);
                                max.y = Math.Max(max.y, v.y);
                                max.z = Math.Max(max.z, v.z);
                            }
                        }
                        if (count == 0)
                            throw new InvalidOperationException("No bounds found on object");
                        var offs = new[] { max / 2 - min / 2, min / 2 - max / 2, default, default, default, default, default, default };
                        offs[2] = new Vector3(offs[0].x, offs[0].y, offs[1].z);
                        offs[3] = new Vector3(offs[0].x, offs[1].y, offs[0].z);
                        offs[4] = new Vector3(offs[0].x, offs[1].y, offs[1].z);
                        offs[5] = new Vector3(offs[1].x, offs[0].y, offs[0].z);
                        offs[6] = new Vector3(offs[1].x, offs[0].y, offs[1].z);
                        offs[7] = new Vector3(offs[1].x, offs[1].y, offs[0].z);
                        c.aspect = (float)innerWidth / innerHeight;
                        var dir = config.cameraAngle * Vector3.forward;
                        if (!config.orthographic)
                            c.fieldOfView = config.renderHeightOverride ?? 90;
                        float s;
                        if (config.orthographic)
                        {
                            var size = Vector3.zero;
                            foreach (var i in offs)
                            {
                                var v = Quaternion.Inverse(config.cameraAngle) * i * 2;
                                size.x = Math.Max(size.x, Math.Abs(v.x));
                                size.y = Math.Max(size.y, Math.Abs(v.y));
                                size.z = Math.Max(size.z, Math.Abs(v.z));
                            }
                            s = (float)Math.Ceiling(Math.Max(size.x / c.aspect, size.y));
                        }
                        else
                        {
                            s = 0;
                            foreach (var i in offs)
                            {
                                var p = Quaternion.Inverse(config.cameraAngle) * i * 2;
                                p.x /= c.aspect;
                                var v = p.magnitude * Math.Sin(Vector3.Angle(p, Vector3.forward) / 180 * Math.PI) / Math.Tan(c.fieldOfView / 2 / 180 * Math.PI) + p.magnitude * Math.Cos(Vector3.Angle(p, Vector3.forward) / 180 * Math.PI);

                                //var v = (i - center).magnitude / Math.Acos(Vector3.Angle(i - center, dir) / 180 * Math.PI) / Math.Tan(c.fieldOfView / 2) + (i - center).magnitude / Math.Asin(Vector3.Angle(i - center, dir) / 180 * Math.PI);
                                if (v > s)
                                    s = (float)v;
                            }
                            s /= 2;
                        }
                        if (float.IsNaN(s) || float.IsInfinity(s) || s == 0)
                            throw new InvalidOperationException("Failed to detect valid object bounds");
                        RenderSettings.ambientLight = config.ambientLight ?? originalLight;
                        c.transform.position = (config.centerOverride ?? ((max + min) / 2)) + (-dir * (s * 2 + cameraBufferDistance));
                        c.transform.rotation = config.cameraAngle;
                        if (config.orthographic)
                            c.orthographicSize = (config.renderHeightOverride ?? s) / 2;
                        c.depthTextureMode = config.depthModeOverride ?? depthTextureMode;
                        c.allowHDR = config.allowHDROverride ?? allowHDR;
                        c.allowMSAA = config.allowMSAAOverride ?? allowMSAA;
                        c.depth = config.cameraDepthOverride ?? depth;
                        c.gateFit = config.gateFitOverride ?? gateFit;
                        c.farClipPlane = s * 8 + cameraBufferDistance;
                        var r = RenderTexture.GetTemporary(innerWidth, innerHeight, 0, RenderTextureFormat.ARGB32, config.linearImg ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
                        c.pixelRect = new Rect(0, 0, r.width * 4, r.height * 4);
                        created.Add(r);
                        c.targetTexture = r;
                        var texture = new Texture2D((int)config.imgWidth, (int)config.imgHeight, TextureFormat.ARGB32, config.mipmapCount, !r.sRGB);
                        if (config.imageMargins != null && config.imageMargins != (0, 0, 0, 0))
                        {
                            var colors = new Color[texture.width * texture.height];
                            for (int i = 0; i < colors.Length; i++)
                                colors[i] = config.backgroundColor;
                        }
                        var prev = RenderTexture.active;
                        RenderTexture.active = r;
                        if (config.imageMargins == null || !(config.complexTransparencyDetection && config.backgroundColor.a < 1))
                        {
                            c.backgroundColor = config.backgroundColor;
                            c.Render();
                            texture.ReadPixels(new Rect(0, 0, r.width, r.height), (int)(config.imageMargins?.left ?? 0), (int)(config.imageMargins?.bottom ?? 0));
                        }
                        else
                        {
                            c.backgroundColor = Color.red;
                            c.Render();
                            texture.ReadPixels(new Rect(0, 0, r.width, r.height), (int)(config.imageMargins?.left ?? 0), (int)(config.imageMargins?.bottom ?? 0));
                            var red = texture.GetPixels(0);
                            c.backgroundColor = Color.green;
                            c.Render();
                            texture.ReadPixels(new Rect(0, 0, r.width, r.height), (int)(config.imageMargins?.left ?? 0), (int)(config.imageMargins?.bottom ?? 0));
                            var green = texture.GetPixels(0);
                            c.backgroundColor = Color.blue;
                            c.Render();
                            texture.ReadPixels(new Rect(0, 0, r.width, r.height), (int)(config.imageMargins?.left ?? 0), (int)(config.imageMargins?.bottom ?? 0));
                            var blue = texture.GetPixels(0);
                            for (int i = 0; i < red.Length; i++)
                                red[i] = GetCommon(red[i], green[i], blue[i]);
                            if (config.imageMargins == null && config.backgroundColor.a > 0)
                                for (int i = 0; i < red.Length; i++)
                                    red[i] = config.backgroundColor.Overlay(red[i]);
                            texture.SetPixels(red, 0);
                        }
                        if (config.imageMargins != null && (config.renderHeightOverride == null || !config.orthographic || config.centerOverride == null))
                        {
                            var edge = texture.FindEdges(config.complexTransparencyDetection ? Color.clear : config.backgroundColor, (int)config.imageMargins.Value.left, (int)config.imageMargins.Value.bottom, (int)config.imageMargins.Value.right, (int)config.imageMargins.Value.top);
                            if (config.centerOverride == null
                                ? (edge.minX > config.imageMargins.Value.left || edge.maxX < config.imgWidth - config.imageMargins.Value.right - 1) && (edge.minY > config.imageMargins.Value.bottom || edge.maxY < config.imgHeight - config.imageMargins.Value.top - 1)
                                : (edge.minX > config.imageMargins.Value.left && edge.maxX < config.imgWidth - config.imageMargins.Value.right - 1 && edge.minY > config.imageMargins.Value.bottom && edge.maxY < config.imgHeight - config.imageMargins.Value.top - 1))
                            {
                                if (config.orthographic)
                                {
                                    if (config.renderHeightOverride == null)
                                    {
                                        c.orthographicSize *=
                                            config.centerOverride == null
                                                ? Math.Max((edge.maxX - edge.minX + 1) / (float)r.width, (edge.maxY - edge.minY + 1) / (float)r.height)
                                                : (1 - Math.Min(Math.Min(edge.minX, r.width - edge.maxX) / r.width, Math.Min(edge.minY, r.height - edge.maxY) / r.height) * 2);
                                    }
                                    if (config.centerOverride == null)
                                        c.transform.position += c.transform.up * ((edge.maxY + edge.minY - r.height) / 2f / r.height * s) + c.transform.right * ((edge.maxX + edge.minX - r.width) / 2f / r.height * s);
                                }
                                else
                                {
                                    if (config.centerOverride == null)
                                    {
                                        c.transform.position += c.transform.rotation * new Vector3((edge.maxX + edge.minX - r.width)* 2f / (float)r.height * s * (float)Math.Tan(c.fieldOfView / 2 / 180 * Math.PI), (edge.maxY + edge.minY - r.height) * 2f / (float)r.height * s * (float)Math.Tan(c.fieldOfView / 2 / 180 * Math.PI), (1 - Math.Max((edge.maxX - edge.minX + 1) / (float)r.width, (edge.maxY - edge.minY + 1) / (float)r.height)) * (s * 2 - cameraBufferDistance));
                                    }
                                    else
                                    {
                                        c.transform.position += c.transform.forward * ((1 - Math.Min(Math.Min(edge.minX, r.width - edge.maxX) / (float)r.width, Math.Min(edge.minY, r.height - edge.maxY) / (float)r.height)) * s * 2);
                                    }
                                }
                                if (config.complexTransparencyDetection && config.backgroundColor.a < 1)
                                {
                                    c.backgroundColor = Color.red;
                                    c.Render();
                                    texture.ReadPixels(new Rect(0, 0, r.width, r.height), (int)(config.imageMargins?.left ?? 0), (int)(config.imageMargins?.bottom ?? 0));
                                    var red = texture.GetPixels(0);
                                    c.backgroundColor = Color.green;
                                    c.Render();
                                    texture.ReadPixels(new Rect(0, 0, r.width, r.height), (int)(config.imageMargins?.left ?? 0), (int)(config.imageMargins?.bottom ?? 0));
                                    var green = texture.GetPixels(0);
                                    c.backgroundColor = Color.blue;
                                    c.Render();
                                    texture.ReadPixels(new Rect(0, 0, r.width, r.height), (int)(config.imageMargins?.left ?? 0), (int)(config.imageMargins?.bottom ?? 0));
                                    var blue = texture.GetPixels(0);
                                    for (int i = 0; i < red.Length; i++)
                                        red[i] = GetCommon(red[i], green[i], blue[i]);
                                    if (config.backgroundColor.a > 0)
                                        for (int i = 0; i < red.Length; i++)
                                            red[i] = config.backgroundColor.Overlay(red[i]);
                                    texture.SetPixels(red, 0);
                                }
                                else
                                {
                                    c.backgroundColor = config.backgroundColor;
                                    c.Render();
                                    texture.ReadPixels(new Rect(0, 0, r.width, r.height), (int)(config.imageMargins?.left ?? 0), (int)(config.imageMargins?.bottom ?? 0));
                                }
                            }
                        }
                        texture.Apply(true);
                        RenderTexture.active = prev;
                        RenderTexture.ReleaseTemporary(r);
                        results[ind] = texture;
                    }
                    catch (Exception e)
                    {
                        exceptions[ind] = e;
                        results[ind] = null;
                    }
                    try { config.AfterRender?.Invoke(o); } catch { }
                    ind++;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                foreach (var i in created)
                    if (i)
                    {
                        if (i is RenderTexture r)
                            RenderTexture.ReleaseTemporary(r);
                        else
                            Object.DestroyImmediate(i);
                    }
                Time.timeScale = originalTimeScale;
                Time.fixedDeltaTime = originalFixedTimeScale;
                RenderSettings.skybox = originalSkybox;
                RenderSettings.ambientLight = originalLight;
                RenderSettings.fog = originalFog;
                foreach (var l in originalLights)
                    l.enabled = true;
                foreach (var p in originalLayers)
                    if (p.Key)
                        p.Key.layer = p.Value;
                foreach (var r in originalForceRenderingOff)
                    if (r)
                        r.forceRenderingOff = true;
            }
            return results;
        }

        static Color GetCommon(Color r, Color g, Color b)
        {
            if (r == g)
                return r;
            if (r == Color.red && g == Color.green)
                return Color.clear;
            var vr = Math.Max(g.r, b.r);
            var vg = Math.Max(r.g, b.g);
            var vb = Math.Max(r.b, g.b);
            var a = Math.Max(Math.Max(1 - g.g + vg, 1 - r.r + vr), 1 - b.b + vb);
            return new Color(vr / a, vg / a, vb / a, a);
        }

        /// <summary>Overlays the <see cref="Color"/> with <paramref name="color"/> with respect to alpha values</summary>
        public static Color Overlay(this Color c, Color color) => new Color(c.r * (1 - color.a) + color.r * color.a, c.g * (1 - color.a) + color.g * color.a, c.b * (1 - color.a) + color.b * color.a, Mathf.Max(c.a, color.a));

        internal static (int minX, int minY, int maxX, int maxY) FindEdges(this Texture2D texture, Color background, int startX = 0, int startY = 0, int endXOffset = 0, int endYOffset = 0)
        {
            var x1 = texture.width - 1;
            var y1 = texture.height - 1;
            var x2 = 0;
            var y2 = 0;
            for (int x = startX; x < texture.width - endXOffset; x++)
                for (int y = startY; y < texture.height - endYOffset; y++)
                    if (background.a == 0 ? texture.GetPixel(x, y).a != 0 : texture.GetPixel(x, y) != background)
                    {
                        x1 = Math.Min(x1, x);
                        x2 = Math.Max(x2, x);
                        y1 = Math.Min(y1, y);
                        y2 = Math.Max(y2, y);
                    }
            return (x1 - startX, y1 - startY, x2 - startX, y2 - startY);
        }
    }
}