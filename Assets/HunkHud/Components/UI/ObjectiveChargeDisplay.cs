using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HunkHud.Components.UI
{
    public class ObjectiveChargeDisplay : MonoBehaviour
    {
        public Image fill;
        public GameObject fullBar;
        public TextMeshProUGUI label;
        public CanvasGroup canvas;

        private float targetAlpha;

        private void Awake()
        {
            this.transform.SetSiblingIndex(1);
        }

        private void FixedUpdate()
        {
            var state = TeleporterInteraction.ActivationState.Idle;
            if (TeleporterInteraction.instance)
                state = TeleporterInteraction.instance.activationState;

            switch (state)
            {
                default:
                    this.targetAlpha = 0f;
                    break;

                case TeleporterInteraction.ActivationState.IdleToCharging:
                    this.fullBar.SetActive(false);
                    this.fill.fillAmount = 0f;
                    this.label.text = "";
                    this.targetAlpha = Mathf.Lerp(this.targetAlpha, 1f, Time.fixedDeltaTime);
                    break;

                case TeleporterInteraction.ActivationState.Charging:
                    this.fullBar.SetActive(false);
                    this.fill.fillAmount = TeleporterInteraction.instance.chargeFraction;
                    this.label.text = $"{TeleporterInteraction.instance.chargePercent}%";
                    this.targetAlpha = 1f;
                    break;

                case TeleporterInteraction.ActivationState.Charged:
                    this.fullBar.SetActive(true);
                    this.fill.fillAmount = 1f;
                    this.label.text = $"100%";
                    this.targetAlpha = Mathf.Lerp(this.targetAlpha, 0f, Time.fixedDeltaTime);
                    break;
            }

            this.canvas.alpha = this.targetAlpha;
            this.canvas.gameObject.SetActive(this.targetAlpha > 0f);
        }
    }
}