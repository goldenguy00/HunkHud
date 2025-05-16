using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HunkHud.Components
{
    [RequireComponent(typeof(CrosshairController))]
    [RequireComponent(typeof(HudElement))]
    public class DynamicCrosshair : MonoBehaviour
    {
        public float range = 300f;
        public float interval = 0.2f;

        private HudElement hudElement;
        private (Image image, Color color)[] crosshairSprites;

        private float stopwatch;

        private void Awake()
        {
            this.hudElement = this.GetComponent<HudElement>();

            var hhhh = new List<(Image, Color)>();

            foreach (var fuckYouDontTellMeToStopUsingIAsMyVariableName in this.GetComponent<CrosshairController>().spriteSpreadPositions)
            {
                var hh = fuckYouDontTellMeToStopUsingIAsMyVariableName.target ? fuckYouDontTellMeToStopUsingIAsMyVariableName.target.GetComponent<Image>() : null;
                if (hh)
                {
                    hhhh.Add((hh, hh.color));
                }
            }

            this.crosshairSprites = hhhh.ToArray();
        }

        private void FixedUpdate()
        {
            this.stopwatch -= Time.fixedDeltaTime;

            if (this.stopwatch <= 0f)
            {
                this.stopwatch = this.interval;

                this.Simulate();
            }
        }

        private void Simulate()
        {
            var viewerBody = this.hudElement ? this.hudElement.targetCharacterBody : null;
            if (!viewerBody || !viewerBody.inputBank)
                return;

            var aimRay = viewerBody.inputBank.GetAimRay();
            Color? color = null;

            // check if there's something in front of the crosshair
            if (Physics.Raycast(aimRay, out var raycastHit, this.range, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Collide))
            {
                var hurtbox = raycastHit.collider ? raycastHit.collider.GetComponent<HurtBox>() : null;
                if (hurtbox)
                {
                    var targetBody = hurtbox.healthComponent ? hurtbox.healthComponent.body : null;
                    if (targetBody && targetBody != viewerBody)
                    {
                        color = targetBody.teamComponent.teamIndex == viewerBody.teamComponent.teamIndex 
                            ? Color.green
                            : Color.red;
                    }
                }
            }

            for (var i = 0; i < this.crosshairSprites.Length; i++)
            {
                this.crosshairSprites[i].image.color = color ?? this.crosshairSprites[i].color;
            }
        }
    }
}