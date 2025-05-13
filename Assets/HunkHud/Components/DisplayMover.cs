using RoR2;
using RoR2.UI;
using UnityEngine;
using System;
using MaterialHud;
using ZioConfigFile;

namespace HunkHud.Components
{
    public abstract class DisplayMover : MonoBehaviour, IConfigHandler
    {
        [NonSerialized]
        public ZioConfigEntry<bool> _configEntry;

        [NonSerialized]
        public HUD targetHud;

        [NonSerialized]
        public CharacterBody targetBody;

        [NonSerialized]
        public CharacterMaster targetMaster;

        protected float delayTimer = 0.1f;

        public float refreshTimer = 2.8f;
        public float activeTimer = 8f;
        public float smoothSpeed = 4f;

        public Vector3 offset;

        [NonSerialized]
        public Vector3 activePosition;

        [NonSerialized]
        public Vector3 desiredPosition;

        public bool isDisplayVisible => this.transform.position == this.desiredPosition && this.desiredPosition != this.activePosition;

        public abstract void CheckForActivity();

        private void ConfigUpdated(ZioConfigEntryBase zioConfigEntryBase, object o, bool arg3)
        {
            this.enabled = this._configEntry.Value;
        }

        public void Startup()
        {
            _configEntry = ConfigHelper.Bind("HunkHud", this.GetType().Name, true, "Enable or disable moving this hud element", (cfg) => this.enabled = cfg.Value);
        }

        public virtual void UpdateReferences(HUD hud, CharacterBody body)
        {
            this.activeTimer = 8f;
            this.targetHud = hud;
            this.targetMaster = hud.targetMaster;
            this.targetBody = body;
        }

        public virtual void SetActive()
        {
            this.activeTimer = Mathf.Max(this.activeTimer, this.refreshTimer);
            this.delayTimer = 0.1f;
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

        protected virtual void OnEnable()
        {
            ConfigUpdated(null, null, arg3: false);
        }

        protected virtual void Update()
        {
            var currentPos = this.transform.localPosition;
            if (this.offset.x == 0f)
                this.desiredPosition.x = currentPos.x;

            if (this.offset.y == 0f)
                this.desiredPosition.y = currentPos.y;

            if (this.offset.y == 0f)
                this.desiredPosition.y = currentPos.y;

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

            this.desiredPosition = this.activePosition;
            if (this.activeTimer <= 0f)
                this.desiredPosition += this.offset;

        }

        protected virtual void OnDestroy()
        {
            _configEntry.SettingChanged -= ConfigUpdated;
        }
    }
}
