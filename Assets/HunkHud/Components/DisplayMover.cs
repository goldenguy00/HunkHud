using RoR2;
using RoR2.UI;
using UnityEngine;
using HunkHud.Modules;
using System;

namespace HunkHud.Components
{
    public class DisplayMover : MonoBehaviour
    {
        [NonSerialized]
        public HUD targetHud;

        [NonSerialized]
        public CharacterBody targetBody;

        [NonSerialized]
        public CharacterMaster targetMaster;

        public float refreshTimer = 2.8f;
        public float activeTimer = 8f;
        public float smoothSpeed = 4f;

        public Vector3 offset;

        private Vector3 activePosition;
        private Vector3 inactivePosition;
        private Vector3 desiredPosition;

        public virtual void UpdateReferences(HUD hud, CharacterBody body)
        {
            this.activeTimer = 8f;
            this.targetHud = hud;
            this.targetMaster = hud.targetMaster;
            this.targetBody = body;
        }

        public virtual void CheckForActivity()
        {
        }

        protected virtual void Start()
        {
            this.activePosition = this.transform.localPosition;
            this.inactivePosition = this.activePosition;
            this.inactivePosition.x += Mathf.Abs(this.offset.x) * Mathf.Sign(this.transform.position.x);
            this.inactivePosition.y += Mathf.Abs(this.offset.y) * Mathf.Sign(this.transform.position.y);
        }

        protected virtual void Update()
        {
            this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, this.desiredPosition, this.smoothSpeed * Time.deltaTime);
        }

        protected virtual void FixedUpdate()
        {
            this.desiredPosition = this.activePosition;
            if (!PluginConfig.dynamicCustomHUD.Value)
                return;

            if (this.activeTimer < this.refreshTimer)
            {
                if (this.targetHud && this.targetHud.scoreboardPanel && this.targetHud.scoreboardPanel.activeSelf)
                    this.activeTimer = this.refreshTimer;
                else
                    CheckForActivity();
            }

            this.activeTimer -= Time.fixedDeltaTime;
            if (this.activeTimer <= 0f)
                this.desiredPosition = this.inactivePosition;
        }
    }
}
