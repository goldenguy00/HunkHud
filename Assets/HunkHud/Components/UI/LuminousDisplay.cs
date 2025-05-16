using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HunkHud.Components.UI
{
    public class LuminousDisplay : CustomHudElement
    {
        public Color activeColor = Color.white;
        public Color inactiveColor = new Color(0f, 0f, 0f, 0.25f);

        public TextMeshProUGUI label;
        public GameObject baseHolder;
        public Image[] pips;

        private int GetBuffCount()
        {
            return this.targetBody.GetBuffCount(DLC2Content.Buffs.IncreasePrimaryDamageBuff);
        }

        private void FixedUpdate()
        {
            var buffCount = this.targetBody ? this.targetBody.GetBuffCount(DLC2Content.Buffs.IncreasePrimaryDamageBuff) : 0;
            if (buffCount == 0)
            {
                this.baseHolder.SetActive(false);
                return;
            }

            for (var i = 0; i < this.pips.Length; i++)
            {
                this.pips[i].color = buffCount > i ? this.activeColor : this.inactiveColor;
            }

            this.label.text = buffCount.ToString();
            this.baseHolder.SetActive(true);
        }
    }
}
