using UnityEngine;
using System;
using MaterialHud;
using ZioConfigFile;
using RoR2.UI;

namespace HunkHud.Components
{
    public abstract class DisplayMover : CustomHudElement, IConfigHandler
    {
        public float delayTimer = 0.1f;
        public float activeInterval = 3f;
        public float activeTimer = 8f;
        public float smoothSpeed = 4f;

        public Vector3 offset;

        public CanvasGroup canvas;

        [NonSerialized]
        public Vector3 activePosition;

        [NonSerialized]
        public Vector3 desiredPosition;

        [NonSerialized]
        public ZioConfigEntry<bool> _configEntry;

        public abstract void CheckForActivity();

        protected virtual void ConfigUpdated(ZioConfigEntryBase zioConfigEntryBase, object o, bool arg3)
        {
            this.enabled = this._configEntry?.Value ?? true;
        }

        public virtual void Startup()
        {
            _configEntry = ConfigHelper.Bind("HunkHud", this.GetType().Name, true, "Enable or disable moving this hud element");
        }

        public void SetActive() => SetActive(this.activeInterval);

        public virtual void SetActive(float time)
        {
            this.activeTimer = Mathf.Max(this.activeTimer, time);
            this.delayTimer = Mathf.Max(this.delayTimer, 0.1f);
        }

        protected override void HUD_onHudTargetChangedGlobal(HUD newHud)
        {
            base.HUD_onHudTargetChangedGlobal(newHud);
            this.SetActive(8f);
        }

        protected virtual void Awake()
        {
            this.canvas = this.GetComponent<CanvasGroup>() ?? this.gameObject.AddComponent<CanvasGroup>();

            Startup();

            _configEntry.SettingChanged += ConfigUpdated;
        }

        protected virtual void Start()
        {
            this.activePosition = this.transform.localPosition;

            this.offset.x = Mathf.Abs(this.offset.x) * Mathf.Sign(this.transform.position.x);
            this.offset.y = Mathf.Abs(this.offset.y) * Mathf.Sign(this.transform.position.y);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            ConfigUpdated(null, null, arg3: false);
        }

        protected virtual void Update()
        {
            var currentPos = this.transform.localPosition;
            var desiredPosition = this.activePosition;

            if (this.activeTimer <= 0f)
                desiredPosition += this.offset;

            if (this.offset.x == 0f)
                desiredPosition.x = currentPos.x;

            if (this.offset.y == 0f)
                desiredPosition.y = currentPos.y;

            if (this.offset.z == 0f)
                desiredPosition.z = currentPos.z;

            this.transform.localPosition = Vector3.Lerp(currentPos, desiredPosition, this.smoothSpeed * Time.deltaTime);

            this.canvas.alpha = Mathf.Clamp01(2f * (this.activeTimer + 0.5f));
        }

        protected virtual void FixedUpdate()
        {
            this.activeTimer -= Time.fixedDeltaTime;
            this.delayTimer -= Time.fixedDeltaTime;

            if (this.hud?.scoreboardPanel && this.hud.scoreboardPanel.activeSelf)
                this.SetActive(this.activeInterval * 0.5f);

            if (this.delayTimer <= 0f)
            {
                this.delayTimer = 0.1f;
                CheckForActivity();
            }
        }

        protected virtual void OnDestroy()
        {
            _configEntry.SettingChanged -= ConfigUpdated;
        }
    }
}
