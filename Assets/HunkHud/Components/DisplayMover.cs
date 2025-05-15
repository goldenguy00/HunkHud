using UnityEngine;
using System;
using MaterialHud;
using ZioConfigFile;

namespace HunkHud.Components
{
    public abstract class DisplayMover : CustomHudElement, IConfigHandler
    {
        [NonSerialized]
        public ZioConfigEntry<bool> _configEntry;

        public float delayTimer = 0.1f;
        public float refreshTimer = 2.8f;
        public float activeTimer = 8f;
        public float smoothSpeed = 4f;

        public Vector3 offset;

        [NonSerialized]
        public Vector3 activePosition;

        [NonSerialized]
        public Vector3 desiredPosition;

        public abstract void CheckForActivity();

        private void ConfigUpdated(ZioConfigEntryBase zioConfigEntryBase, object o, bool arg3)
        {
            this.enabled = this._configEntry?.Value ?? true;
        }

        public void Startup()
        {
            _configEntry = ConfigHelper.Bind("HunkHud", this.GetType().Name, true, "Enable or disable moving this hud element");
        }

        public void UpdateReferences()
        {
            this.activeTimer = 8f;
            this.delayTimer = this.activeTimer - this.refreshTimer;
        }

        public virtual void SetActive()
        {
            this.activeTimer = Mathf.Max(this.activeTimer, this.refreshTimer);
            this.delayTimer = Mathf.Max(this.delayTimer, 0.1f);
        }

        protected virtual void Awake()
        {
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
        }

        protected virtual void FixedUpdate()
        {
            this.activeTimer -= Time.fixedDeltaTime;
            this.delayTimer -= Time.fixedDeltaTime;

            if (this.targetHud && this.targetHud.scoreboardPanel && this.targetHud.scoreboardPanel.activeSelf)
                this.SetActive();

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
