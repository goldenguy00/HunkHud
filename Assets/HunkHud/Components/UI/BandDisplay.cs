using System;
using RoR2;
using UnityEngine;
using UnityEngine.UI;

namespace HunkHud.Components.UI
{
    public class BandDisplay : MonoBehaviour
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

        private BuffIndex readyBuffIndex = BuffIndex.None;
        private BuffIndex cooldownBuffIndex = BuffIndex.None;

        private CharacterBody targetBody => this.displayParent.targetBody;
        private BandDisplayMover displayParent;

        private float timer;
        private int maxBuffs;

        private void Start()
        {
            this.displayParent = this.GetComponentInParent<BandDisplayMover>();
            this.readyBuffIndex = BuffCatalog.FindBuffIndex(this.buffName);
            this.cooldownBuffIndex = BuffCatalog.FindBuffIndex(this.cooldownName);
        }

        private void FixedUpdate()
        {
            if (this.displayParent && this.targetBody)
            {
                if (this.targetBody.HasBuff(this.readyBuffIndex))
                {
                    SetRingReady();
                    return;
                }

                var newBuffs = this.targetBody.GetBuffCount(this.cooldownBuffIndex);
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
            this.timer = 0f;

            this.fullObj.SetActive(true);
            this.fillObj.SetActive(false);
        }

        private void SetRingCooldown(int newBuffs)
        {
            this.fullObj.SetActive(false);
            this.fillObj.SetActive(true);

            this.maxBuffs = Math.Max(this.maxBuffs, newBuffs);

            var timeLeft = Util.Remap(this.timer, 0f, this.maxBuffs, 0f, 1f);
            this.fillImage.color = this.fillGradient.Evaluate(timeLeft);
            this.fillImage.fillAmount = timeLeft;

            this.timer += Time.fixedDeltaTime;
            this.displayParent.activeTimer = Mathf.Max(this.displayParent.activeTimer, this.displayParent.refreshTimer);
        }
    }
}
