using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HunkHud.Components
{
    public class DynamicCrosshair : MonoBehaviour
    {
        public float range = 300f;
        public float interval = 0.2f;

        private CrosshairController crosshairController;
        private Image[] crosshairSprites;
        private float stopwatch;

        private void Awake()
        {
            this.crosshairController = this.GetComponent<CrosshairController>();

            var hhhh = new List<Image>();

            foreach (var fuckYouDontTellMeToStopUsingIAsMyVariableName in this.crosshairController.spriteSpreadPositions)
            {
                if (fuckYouDontTellMeToStopUsingIAsMyVariableName.target)
                    hhhh.Add(fuckYouDontTellMeToStopUsingIAsMyVariableName.target.GetComponent<Image>());
            }

            this.crosshairSprites = hhhh.ToArray();
        }

        private void FixedUpdate()
        {
            this.stopwatch -= Time.fixedDeltaTime;

            if (this.stopwatch <= 0f)
                this.Simulate();
        }

        private void Simulate()
        {
            this.stopwatch = this.interval;

            if (!this.crosshairController || !this.crosshairController.hudElement)
                return;

            var body = this.crosshairController.hudElement.targetCharacterBody;
            if (!body || !body.inputBank || !body.hasEffectiveAuthority)
                return;

            var aimRay = body.inputBank.GetAimRay();
            var color = Color.white;

            // check if there's something in front of the crosshair
            if (Physics.Raycast(aimRay, out var raycastHit, this.range, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Collide) && raycastHit.collider)
            {
                var hurtbox = raycastHit.collider.GetComponent<HurtBox>();
                if (hurtbox)
                {
                    var targetBody = hurtbox.healthComponent ? hurtbox.healthComponent.body : null;
                    if (targetBody && targetBody != body)
                    {
                        color = targetBody.teamComponent.teamIndex == body.teamComponent.teamIndex 
                            ? Color.green
                            : Color.red;
                    }
                }
            }

            ColorCrosshair(color);
        }

        private void ColorCrosshair(Color newColor)
        {
            for (var i = 0; i < this.crosshairSprites.Length; i++)
            {
                if (this.crosshairSprites[i])
                    this.crosshairSprites[i].color = newColor;
            }
        }
    }
}