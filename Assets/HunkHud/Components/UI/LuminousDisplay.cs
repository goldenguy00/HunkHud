using System;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HunkHud.Components.UI
{
    public class LuminousDisplay : MonoBehaviour
    {
        public static LuminousDisplay instance;
        public Color activeColor = Color.white;
        public Color inactiveColor = new Color(0f, 0f, 0f, 0.25f);

        public TextMeshProUGUI label;
        public GameObject baseHolder;
        public Image[] pips;

        [NonSerialized]
        public CharacterBody targetBody;

        private bool hasLuminousShot
        {
            get
            {
                if (!this.targetBody || !this.targetBody.inventory)
                    return false;

                return this.targetBody.HasBuff(DLC2Content.Buffs.IncreasePrimaryDamageBuff) || this.targetBody.inventory.GetItemCount(DLC2Content.Items.IncreasePrimaryDamage) > 0;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private void FixedUpdate()
        {
            if (!this.hasLuminousShot)
            {
                this.baseHolder.SetActive(false);
                return;
            }

            int buffCount = this.targetBody.GetBuffCount(DLC2Content.Buffs.IncreasePrimaryDamageBuff);
            for (var i = 0; i < this.pips.Length; i++)
            {
                this.pips[i].color = buffCount > i ? this.activeColor : this.inactiveColor;
            }

            this.label.text = buffCount.ToString();
            this.baseHolder.SetActive(true);
        }
    }
}
