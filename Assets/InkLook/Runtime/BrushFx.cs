using UnityEngine;

namespace InkLook
{
    /// <summary>
    /// Brush strokes in the world: arcs, rings, streaks, bursts and splats, drawn with the Ink/Brush shader.
    /// Every stroke paints in along its length and then dries away. Strokes keep animating in slow motion.
    /// Put one in the scene (Tools > Ink Look > Add Ink World to Scene does it) and call the static methods.
    /// Calls are ignored when there is no BrushFx in the scene.
    /// </summary>
    public class BrushFx : MonoBehaviour
    {
        public static BrushFx I;
        public Material brushWet;
        public Material brushDry;
        public Material splat;
        [Tooltip("Strokes are drawn flat on this world Z, in front of a side-on scene.")]
        public float zPlane = -0.8f;
        [Tooltip("Used if no InkWorld has set the grain texture.")]
        public Texture2D grain;
        public static readonly Color Ink = new Color(0.07f, 0.065f, 0.06f, 1f);
        public static readonly Color Wash = new Color(0.32f, 0.30f, 0.28f, 0.9f);

        void Awake()
        {
            I = this;
            InkWorld.EnsureGrain(grain);
        }

        static BrushStroke Make(string name, Mesh mesh, Material mat, Vector3 pos, Color col, float reveal, float life)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            var s = go.AddComponent<BrushStroke>();
            s.Init(r, mesh, col, reveal, life);
            return s;
        }

        /// <summary>A curved stroke from one angle to another (degrees, counter-clockwise from +X).</summary>
        public static BrushStroke Arc(Vector3 centre, float radius, float fromDeg, float toDeg, float width, Color col, float life, bool dry = false)
        {
            if (I == null) return null;
            const int n = 30;
            var v = new Vector3[(n + 1) * 2];
            var uv = new Vector2[(n + 1) * 2];
            var tris = new int[n * 6];
            for (int i = 0; i <= n; i++)
            {
                float t = (float)i / n;
                float a = Mathf.Lerp(fromDeg, toDeg, t) * Mathf.Deg2Rad;
                Vector3 d = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
                v[i * 2] = d * (radius - width * 0.5f);
                v[i * 2 + 1] = d * (radius + width * 0.5f);
                uv[i * 2] = new Vector2(t, 0f);
                uv[i * 2 + 1] = new Vector2(t, 1f);
                if (i < n)
                {
                    int k = i * 6, b = i * 2;
                    tris[k] = b; tris[k + 1] = b + 1; tris[k + 2] = b + 2;
                    tris[k + 3] = b + 1; tris[k + 4] = b + 3; tris[k + 5] = b + 2;
                }
            }
            var m = new Mesh();
            m.vertices = v; m.uv = uv; m.triangles = tris;
            m.RecalculateBounds();
            centre.z = I.zPlane;
            return Make("BrushArc", m, dry ? I.brushDry : I.brushWet, centre, col, life * 0.32f, life);
        }

        /// <summary>An enso: a near-closed ring that can grow as it dries. Used for the parry.</summary>
        public static BrushStroke Ring(Vector3 centre, float radius, float width, Color col, float life, float grow)
        {
            float start = Random.Range(60f, 140f);
            var s = Arc(centre, radius, start, start - 335f, width, col, life);
            if (s != null) s.grow = grow;
            return s;
        }

        /// <summary>A straight stroke between two points.</summary>
        public static BrushStroke Streak(Vector3 a, Vector3 b, float width, Color col, float life, bool dry = true)
        {
            if (I == null) return null;
            Vector3 d = b - a; d.z = 0f;
            float len = d.magnitude;
            if (len < 0.001f) return null;
            Vector3 n = new Vector3(-d.y, d.x, 0f) / len * (width * 0.5f);
            var m = new Mesh();
            m.vertices = new[] { -n, n, d - n, d + n };
            m.uv = new[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 0f), new Vector2(1f, 1f) };
            m.triangles = new[] { 0, 1, 2, 1, 3, 2 };
            m.RecalculateBounds();
            a.z = I.zPlane;
            return Make("BrushStreak", m, dry ? I.brushDry : I.brushWet, a, col, life * 0.25f, life);
        }

        /// <summary>Short strokes flying outward from a point: the block spark.</summary>
        public static void Burst(Vector3 centre, int count, float inner, float outer, float width, Color col, float life, Vector2 bias = default(Vector2))
        {
            float baseAngle = Random.value * 360f;
            for (int i = 0; i < count; i++)
            {
                float a = (baseAngle + 360f * i / count + Random.Range(-18f, 18f)) * Mathf.Deg2Rad;
                Vector3 d = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) + (Vector3)bias;
                d.Normalize();
                Streak(centre + d * inner, centre + d * (outer * Random.Range(0.7f, 1.15f)), width * Random.Range(0.7f, 1.3f), col, life * Random.Range(0.8f, 1.2f));
            }
        }

        /// <summary>A blot of ink.</summary>
        public static BrushStroke Splat(Vector3 centre, float size, Color col, float life)
        {
            if (I == null || I.splat == null) return null;
            float h = size * 0.5f;
            var m = new Mesh();
            m.vertices = new[] { new Vector3(-h, -h, 0f), new Vector3(-h, h, 0f), new Vector3(h, -h, 0f), new Vector3(h, h, 0f) };
            m.uv = new[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 0f), new Vector2(1f, 1f) };
            m.triangles = new[] { 0, 1, 2, 1, 3, 2 };
            m.RecalculateBounds();
            centre.z = I.zPlane + 0.05f;
            var s = Make("BrushSplat", m, I.splat, centre, col, 0.0001f, life);
            s.transform.rotation = Quaternion.Euler(0f, 0f, Random.value * 360f);
            s.transform.localScale = Vector3.one * 0.55f;
            s.popTo = 1f;
            return s;
        }
    }

    public class BrushStroke : MonoBehaviour
    {
        public float grow;
        public float popTo;
        Renderer rend;
        Mesh mesh;
        MaterialPropertyBlock mpb;
        Color colour;
        float revealTime, life, t;
        static readonly int RevealId = Shader.PropertyToID("_Reveal");
        static readonly int DryId = Shader.PropertyToID("_Dry");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        public void Init(Renderer r, Mesh m, Color col, float reveal, float lifetime)
        {
            rend = r; mesh = m; colour = col; revealTime = Mathf.Max(0.0001f, reveal); life = lifetime;
            mpb = new MaterialPropertyBlock();
            Apply();
        }

        void Apply()
        {
            float reveal = Mathf.Clamp01(t / revealTime) * 1.2f;
            float dryStart = Mathf.Max(revealTime, life * 0.35f);
            float dry = Mathf.Clamp01((t - dryStart) / Mathf.Max(0.001f, life - dryStart));
            mpb.SetFloat(RevealId, reveal);
            mpb.SetFloat(DryId, dry * dry);
            mpb.SetColor(ColorId, colour);
            rend.SetPropertyBlock(mpb);
        }

        void Update()
        {
            // Unscaled: strokes keep painting through freeze frames and slow motion.
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            t += dt;
            if (grow != 0f) transform.localScale *= 1f + grow * dt;
            if (popTo > 0f) transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * popTo, 1f - Mathf.Exp(-30f * dt));
            Apply();
            if (t >= life) Destroy(gameObject);
        }

        void OnDestroy() { if (mesh != null) Destroy(mesh); }
    }
}
