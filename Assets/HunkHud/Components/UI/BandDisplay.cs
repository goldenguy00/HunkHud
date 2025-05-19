using System;
using RoR2;
using UnityEngine;
using UnityEngine.UI;

namespace HunkHud.Components.UI
{
    public class BandDisplay : CustomHudElement
    {
        public Gradient fillGradient = new Gradient
        {
            alphaKeys =
            new GradientAlphaKey[] {
                new GradientAlphaKey
                {
                    alpha = 1f,
                    time = 0f
                },
                new GradientAlphaKey
                {
                    alpha = 1f,
                    time = 1f
                }
            },
            colorKeys =
            new GradientColorKey[] {
                new GradientColorKey
                {
                    color = Color.red,
                    time = 0f
                },
                new GradientColorKey
                {
                    color = Color.yellow,
                    time = 0.5f
                },
                new GradientColorKey
                {
                    color = new Color(31f / 85f, 57f / 85f, 23f / 51f),
                    time = 0.6f
                },
                new GradientColorKey
                {
                    color = new Color(31f / 85f, 57f / 85f, 23f / 51f),
                    time = 0.95f
                },
                new GradientColorKey
                {
                    color = Color.white,
                    time = 1f
                }
            }
        };

        public GameObject fullObj;
        public GameObject fillObj;
        public Image fillImage;

        public string buffName;
        public string cooldownName;

        [NonSerialized]
        public HealthBarMover healthBar;

        private float timer;
        private int maxBuffs;

        private void FixedUpdate()
        {
            if (this.targetBody && this.healthBar)
            {
                if (this.targetBody.HasBuff(BuffCatalog.FindBuffIndex(this.buffName)))
                {
                    SetRingReady();
                    return;
                }

                var newBuffs = this.targetBody.GetBuffCount(BuffCatalog.FindBuffIndex(this.cooldownName));
                if (newBuffs > 0)
                {
                    SetRingCooldown(newBuffs);
                    return;
                }
            }

            this.fillObj.SetActive(false);
            this.fullObj.SetActive(false);
        }

        private void SetRingReady()
        {
            this.fullObj.SetActive(true);
            this.fillObj.SetActive(false);

            this.timer = 0f;
            this.maxBuffs = 0;
        }

        private void SetRingCooldown(int newBuffs)
        {
            this.fullObj.SetActive(false);
            this.fillObj.SetActive(true);
            this.healthBar.SetActive();

            this.maxBuffs = Math.Max(this.maxBuffs, newBuffs);
            var timeLeft = Util.Remap(this.timer, 0f, this.maxBuffs, 0f, 1f);
            this.fillImage.color = this.fillGradient.Evaluate(timeLeft);
            this.fillImage.fillAmount = timeLeft;

            this.timer += Time.fixedDeltaTime;
        }
    }
}
