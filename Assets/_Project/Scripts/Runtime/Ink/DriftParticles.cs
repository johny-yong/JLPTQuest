using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    /*
        Little bits of life in the scene: fireflies at night, petals by day, birds at dawn and dusk, embers in battle.
        A small pool of UI images moved by hand, so it sits in the same canvas order as everything else.
        Calm mode keeps fewer of them and slows them down.
    */
    [RequireComponent(typeof(RectTransform))]
    public class DriftParticles : MonoBehaviour
    {
        [SerializeField] private Sprite mote;
        [SerializeField] private Sprite petal;
        [SerializeField] private Sprite bird;
        [SerializeField] private int count = 10;

        private class Speck
        {
            public Image image;
            public Vector2 position;
            public Vector2 velocity;
            public float phase;
            public float size;
            public float spin;
        }

        private readonly List<Speck> specks = new List<Speck>();
        private RectTransform area;
        private DriftKind kind = DriftKind.None;
        private Color tint = Color.white;

        public void Show(DriftKind newKind, Color newTint)
        {
            if (area == null) area = (RectTransform)transform;
            bool changed = newKind != kind;
            kind = newKind;
            tint = newTint;
            if (specks.Count == 0) Build();

            int live = Live();
            for (int i = 0; i < specks.Count; i++)
            {
                Speck speck = specks[i];
                speck.image.gameObject.SetActive(kind != DriftKind.None && i < live);
                if (changed) Spawn(speck, true);
            }
        }

        private int Live()
        {
            int wanted = kind == DriftKind.Birds ? 3 : count;
            return GameSettings.CalmMode ? Mathf.Max(1, wanted / 2) : wanted;
        }

        private void Build()
        {
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Speck", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(transform, false);
                var image = go.GetComponent<Image>();
                image.raycastTarget = false;
                var rect = image.rectTransform;
                rect.anchorMin = rect.anchorMax = Vector2.zero;
                specks.Add(new Speck { image = image });
            }
        }

        //A fresh speck somewhere it makes sense for its kind. First fills the whole area, later ones come in from an edge
        private void Spawn(Speck speck, bool first)
        {
            Rect r = area.rect;
            float w = r.width, h = r.height;
            speck.phase = Random.value * 10f;
            switch (kind)
            {
                case DriftKind.Fireflies:
                    speck.image.sprite = mote;
                    speck.size = Random.Range(14f, 26f);
                    speck.position = new Vector2(Random.value * w, Random.Range(0.05f, 0.55f) * h);
                    speck.velocity = Random.insideUnitCircle * 14f;
                    break;
                case DriftKind.Petals:
                    speck.image.sprite = petal;
                    speck.size = Random.Range(16f, 28f);
                    speck.position = first ? new Vector2(Random.value * w, Random.value * h) : new Vector2(Random.Range(-0.1f, 0.9f) * w, h + 20f);
                    speck.velocity = new Vector2(Random.Range(20f, 45f), -Random.Range(35f, 60f));
                    speck.spin = Random.Range(-90f, 90f);
                    break;
                case DriftKind.Birds:
                    speck.image.sprite = bird;
                    speck.size = Random.Range(34f, 52f);
                    speck.position = new Vector2(first ? Random.value * w : -60f, Random.Range(0.62f, 0.9f) * h);
                    speck.velocity = new Vector2(Random.Range(22f, 38f), Random.Range(-3f, 3f));
                    break;
                case DriftKind.Embers:
                    speck.image.sprite = mote;
                    speck.size = Random.Range(6f, 12f);
                    speck.position = new Vector2(Random.value * w, first ? Random.value * h : -10f);
                    speck.velocity = new Vector2(Random.Range(-8f, 8f), Random.Range(18f, 40f));
                    break;
            }
            float aspect = speck.image.sprite != null ? speck.image.sprite.rect.width / speck.image.sprite.rect.height : 1f;
            speck.image.rectTransform.sizeDelta = new Vector2(speck.size * aspect, speck.size);
            speck.image.rectTransform.localRotation = Quaternion.identity;
        }

        void Update()
        {
            if (kind == DriftKind.None || specks.Count == 0) return;
            float dt = Time.unscaledDeltaTime * (GameSettings.CalmMode ? 0.6f : 1f);
            float t = Time.unscaledTime;
            Rect r = area.rect;
            int live = Live();

            for (int i = 0; i < live && i < specks.Count; i++)
            {
                Speck s = specks[i];
                float alpha = 1f;
                switch (kind)
                {
                    case DriftKind.Fireflies:
                        //Wander, turning slowly, and blink
                        s.velocity = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.7f + s.phase) * 40f * dt) * s.velocity;
                        alpha = Mathf.Clamp01(0.35f + 0.65f * Mathf.Sin(t * 1.6f + s.phase));
                        //They turn back at the edges instead of leaving
                        if ((s.position.x < 0f && s.velocity.x < 0f) || (s.position.x > r.width && s.velocity.x > 0f)) s.velocity.x = -s.velocity.x;
                        if ((s.position.y < 0f && s.velocity.y < 0f) || (s.position.y > r.height * 0.6f && s.velocity.y > 0f)) s.velocity.y = -s.velocity.y;
                        break;
                    case DriftKind.Petals:
                        s.velocity.x += Mathf.Sin(t * 1.2f + s.phase) * 10f * dt;
                        s.image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, s.spin * t + s.phase * 30f);
                        break;
                    case DriftKind.Birds:
                        //Wings: squash the stroke up and down
                        s.image.rectTransform.localScale = new Vector3(1f, 0.6f + 0.4f * Mathf.Abs(Mathf.Sin(t * 3f + s.phase)), 1f);
                        break;
                    case DriftKind.Embers:
                        alpha = Mathf.Clamp01(1f - s.position.y / Mathf.Max(1f, r.height));
                        break;
                }
                s.position += s.velocity * dt;
                s.image.rectTransform.anchoredPosition = s.position;
                Color c = tint;
                c.a *= alpha;
                s.image.color = c;

                if (Outside(s.position, r)) Spawn(s, false);
            }
        }

        private bool Outside(Vector2 p, Rect r)
        {
            const float margin = 80f;
            if (kind == DriftKind.Fireflies) return false;
            return p.x < -margin || p.x > r.width + margin || p.y < -margin || p.y > r.height + margin;
        }
    }
}
