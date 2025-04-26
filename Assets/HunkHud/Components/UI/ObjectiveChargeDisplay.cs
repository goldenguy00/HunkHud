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

        private void FixedUpdate()
        {
            var state = TeleporterInteraction.ActivationState.Idle;
            if (TeleporterInteraction.instance)
                state = TeleporterInteraction.instance.activationState;

            switch (state)
            {
                default:
                    this.canvas.alpha = 0f;
                    break;

                case TeleporterInteraction.ActivationState.IdleToCharging:
                    this.fullBar.SetActive(false);
                    this.fill.fillAmount = 0f;
                    this.label.text = "";
                    this.canvas.alpha = Mathf.Lerp(this.canvas.alpha, 1f, Time.fixedDeltaTime);
                    break;

                case TeleporterInteraction.ActivationState.Charging:
                    this.fullBar.SetActive(false);
                    this.fill.fillAmount = TeleporterInteraction.instance.chargeFraction;
                    this.label.text = $"{TeleporterInteraction.instance.chargePercent}%";
                    this.canvas.alpha = 1f;
                    break;

                case TeleporterInteraction.ActivationState.Charged:
                    this.fullBar.SetActive(true);
                    this.fill.fillAmount = 1f;
                    this.label.text = $"100%";
                    this.canvas.alpha = Mathf.Lerp(this.canvas.alpha, 0f, Time.fixedDeltaTime);
                    break;
            }
        }
    }
}