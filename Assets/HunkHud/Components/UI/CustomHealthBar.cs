using System;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HunkHud.Components.UI
{
    public class CustomHealthBar : HealthBar
    {
        public static CustomHealthBar instance;

        [Serializable]
        public struct CustomBarInfo
        {
            [NonSerialized]
            public bool enabled;
            [NonSerialized]
            public float targetFill;
            [NonSerialized]
            public float currentFill;
            [SerializeField]
            public Image image;
        }    

        public Gradient healthBarGradient = new Gradient
        {
            alphaKeys = new GradientAlphaKey[]
            {
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
            colorKeys = new GradientColorKey[]
            {
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
                    time = 0.8f
                },
                new GradientColorKey
                {
                    color = new Color(31f / 85f, 57f / 85f, 23f / 51f),
                    time = 1f
                }
            }
        };

        public Color shieldColor = new Color(0.14901961f, 0.70980394f, 0.80784315f);
        public Color pinkShieldColor = new Color(0.80784315f, 0.29803923f, 0.7607843f);
        public Color infusionColor = new Color(1f, 18f / 85f, 0.23137255f);

        public CustomBarInfo healthFill;
        public CustomBarInfo lowHealthFill;

        public CustomBarInfo shieldFill;
        public CustomBarInfo damageFill;

        public CustomBarInfo healingFill;
        public CustomBarInfo curseFill;
        public CustomBarInfo barrierFill;
        public CustomBarInfo barrierFillShiny;
        public CustomBarInfo delayedDamageMask;
        public CustomBarInfo ospFill;
        public CustomBarInfo echoFill;
        public CustomBarInfo collapseFill;
        public CustomBarInfo cullFill;

        public TextMeshProUGUI gunText;
        public RawImage gunIcon;
        public GameObject gunIconHolder;

        public RawImage characterIcon;
        public RawImage characterIconBorder;
        public GameObject characterIconHolder;

        public GameObject lunarRuinDisplay;
        public GameObject immunityDisplay;

        public GameObject biomassBar;

        public HealthBarMover hpBarMover;

        public CharacterBody targetBody => this.source?.body;

        private float fillSpeed = 1.5f;
        private float minFill = 0.341f;
        private float maxFill = 0.752f;
        private float inverseFillMin = 0.248f;
        private float inverseFillMax = 0.659f;

        private new void Awake()
        {
            base.Awake();

            this.SetDefaults();

            this.biomassBar?.SetActive(false);
            this.gunIconHolder?.SetActive(false);
            this.characterIconHolder?.SetActive(true);
        }

        private new void OnEnable()
        {
            base.OnEnable();

            SingletonHelper.Assign(ref instance, this);
        }

        private new void OnDisable()
        {
            base.OnDisable();

            SingletonHelper.Unassign(ref instance, this);
        }

        private new void Update()
        {
            updateTimer -= Time.deltaTime;
            if (_source != oldSource)
            {
                updateTimer = 0f;
                oldSource = _source;
            }

            if (updateTimer <= 0f)
            {
                updateTimer = updateDelay;

                UpdateCustomBarInfos();
                ApplyCustomBars();
            }

            ApplyCustomBarsUpdate();
        }

        private void ApplyCustomBarsUpdate()
        {
            Apply(ref healthFill);
            Apply(ref shieldFill);
            Apply(ref damageFill);
        }

        private void ApplyCustomBars()
        {
            Apply(ref healingFill);
            Apply(ref barrierFill);
            Apply(ref barrierFillShiny);
            Apply(ref lowHealthFill);
            Apply(ref curseFill);
            Apply(ref cullFill);

            Apply(ref delayedDamageMask);
            Apply(ref collapseFill);
            Apply(ref echoFill);
            Apply(ref ospFill);
        }

        private void Apply(ref CustomBarInfo info)
        {
            if (info.currentFill != info.targetFill)
                info.currentFill = Mathf.Lerp(info.currentFill, info.targetFill, this.fillSpeed * Time.deltaTime);

            info.image.enabled = info.enabled;
            info.image.fillAmount = info.currentFill;
        }

        private void SetDefaults()
        {
            this.healthFill.enabled = false;
            this.healingFill.enabled = false;
            this.lowHealthFill.enabled = false;

            this.shieldFill.enabled = false;
            this.barrierFill.enabled = false;
            this.barrierFillShiny.enabled = false;

            this.damageFill.targetFill = 0f;
            this.damageFill.enabled = 0f < this.damageFill.currentFill;

            this.delayedDamageMask.enabled = false;
            this.collapseFill.enabled = false;
            this.echoFill.enabled = false;
            this.ospFill.enabled = false;
            this.cullFill.enabled = false;

            this.immunityDisplay?.SetActive(false);
        }

        private void UpdateCustomBarInfos()
        {
            if (!this.source || !this.targetBody)
            {
                SetDefaults();
                return;
            }

            if (isInventoryCheckDirty)
            {
                CheckInventory();
            }

            HealthComponent.HealthBarValues barValues = this.source.GetHealthBarValues();
            var missingHealthFraction = 1f - (barValues.healthFraction + barValues.shieldFraction);
            var reducedHealthFraction = 1f - barValues.curseFraction;

            var collapseFraction = Mathf.Clamp01(this.GetCollapseFraction() * reducedHealthFraction);

            this.shieldFill.image.color = barValues.hasVoidShields ? this.pinkShieldColor : this.shieldColor;
            this.healthFill.image.color = barValues.hasInfusion ? this.infusionColor : this.healthBarGradient.Evaluate(barValues.healthFraction);

            this.immunityDisplay?.SetActive(this.targetBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility));
            this.lunarRuinDisplay?.SetActive(barValues.hasLunarRuin);

            // lerped bars

            var healthFillAmount = Util.Remap(barValues.healthFraction, 0f, 1f, this.minFill, this.maxFill);
            this.healthFill.enabled = barValues.healthFraction > 0f;
            this.healthFill.targetFill = healthFillAmount;
            this.healthFill.currentFill = Mathf.Min(healthFillAmount, this.healthFill.currentFill);

            var combinedHealthFillAmount = Util.Remap(barValues.healthFraction + barValues.shieldFraction, 0f, 1f, this.minFill, this.maxFill);
            this.damageFill.targetFill = combinedHealthFillAmount;
            this.damageFill.currentFill = Mathf.Max(combinedHealthFillAmount, this.damageFill.currentFill);
            this.damageFill.enabled = this.damageFill.currentFill > combinedHealthFillAmount;

            this.shieldFill.enabled = barValues.shieldFraction > 0f;
            this.shieldFill.targetFill = combinedHealthFillAmount;
            this.shieldFill.currentFill = Mathf.Min(combinedHealthFillAmount, this.shieldFill.currentFill);

            // remap
            Remap(ref this.healingFill, barValues.healthFraction, enabled: this.healthFill.currentFill < this.healthFill.targetFill, inverse: false);
            Remap(ref this.lowHealthFill, HealthComponent.lowHealthFraction * reducedHealthFraction, enabled: (this.hasLowHealthItem || this.hasLowHealthBuff) && !this.source.isHealthLow, inverse: false);

            Remap(ref this.barrierFill, barValues.barrierFraction, enabled: barValues.barrierFraction > 0f, inverse: false);
            Remap(ref this.barrierFillShiny, barValues.barrierFraction, enabled: barValues.barrierFraction > 0f, inverse: false);

            Remap(ref this.cullFill, barValues.cullFraction, enabled: barValues.cullFraction > 0f, inverse: false);
            Remap(ref this.curseFill, barValues.curseFraction, enabled: barValues.curseFraction > 0f, inverse: true);
            Remap(ref this.ospFill, barValues.ospFraction + missingHealthFraction, enabled: barValues.ospFraction > 0f, inverse: true);

            Remap(ref this.delayedDamageMask, barValues.healthFraction + barValues.shieldFraction, enabled: true, inverse: false);
            Remap(ref this.collapseFill, collapseFraction + missingHealthFraction, enabled: collapseFraction > 0f && this.targetBody.HasBuff(DLC1Content.Buffs.Fracture), inverse: true);
            Remap(ref this.echoFill, barValues.echoFraction + missingHealthFraction, enabled: barValues.echoFraction > 0f && this.targetBody.HasBuff(DLC2Content.Buffs.DelayedDamageDebuff), inverse: true);

            if (this.echoFill.enabled || this.collapseFill.enabled)
            {
                this.hpBarMover?.SetActive();
                this.damageFill.currentFill = this.damageFill.targetFill;
            }

            void Remap(ref CustomBarInfo bar, float value, bool enabled, bool inverse)
            {
                bar.enabled = enabled;
                value = Mathf.Clamp01(value);
                var fillAmount = inverse ? Util.Remap(value, 0f, 1f, this.inverseFillMin, this.inverseFillMax) : Util.Remap(value, 0f, 1f, this.minFill, this.maxFill);
                bar.targetFill = fillAmount;
                bar.currentFill = fillAmount;
            }
        }

        private float GetCollapseFraction()
        {
            var dotController = DotController.FindDotController(this.targetBody.gameObject);
            if (!dotController || !dotController.HasDotActive(DotController.DotIndex.Fracture))
                return 0f;

            float collapseDamage = 0f;

            foreach (DotController.DotStack dotStack in dotController.dotStackList)
            {
                if (dotStack.dotIndex != DotController.DotIndex.Fracture)
                    continue;

                float stackDamage = dotStack.damage;
                ModifyIncomingDamage(ref stackDamage, dotStack.attackerObject, dotStack.attackerTeam, dotStack.damageType);
                
                collapseDamage += Mathf.Max(0f, stackDamage);
            }

            return collapseDamage / this.source.fullCombinedHealth;
        }

        private void ModifyIncomingDamage(ref float damage, GameObject attacker, TeamIndex attackerTeam, DamageTypeCombo damageType)
        {
            if ((damageType.damageTypeExtended & DamageTypeExtended.DamagePercentOfMaxHealth) != 0)
            {
                damage = this.source.fullHealth * 0.1f;
            }

            if (this.targetBody.HasBuff(DLC2Content.Buffs.KnockUpHitEnemiesJuggleCount))
            {
                damage *= 1f + (0.1f * this.targetBody.GetBuffCount(DLC2Content.Buffs.KnockUpHitEnemiesJuggleCount));
            }

            if (this.targetBody.HasBuff(DLC2Content.Buffs.lunarruin))
            {
                damage *= 1f + (0.1f * this.targetBody.GetBuffCount(DLC2Content.Buffs.lunarruin));
            }

            if (attackerTeam == this.targetBody.teamComponent.teamIndex)
            {
                damage *= TeamCatalog.GetTeamDef(attackerTeam)?.friendlyFireScaling ?? 1f;
            }

            int itemCount;
            var attackerBody = attacker ? attacker.GetComponent<CharacterBody>() : null;
            if (attackerBody && attackerBody.inventory)
            {
                if (this.source.combinedHealth >= this.source.fullCombinedHealth * 0.9f)
                {
                    itemCount = attackerBody.inventory.GetItemCount(RoR2Content.Items.Crowbar);
                    if (itemCount > 0)
                    {
                        damage *= 1f + (0.75f * itemCount);
                    }
                }

                itemCount = attackerBody.inventory.GetItemCount(RoR2Content.Items.NearbyDamageBonus);
                if (itemCount > 0 && (this.targetBody.corePosition - attackerBody.corePosition).sqrMagnitude <= 13f * 13f)
                {
                    damage *= 1f + (itemCount * 0.2f);
                }

                itemCount = attackerBody.inventory.GetItemCount(DLC1Content.Items.FragileDamageBonus);
                if (itemCount > 0)
                {
                    damage *= 1f + (itemCount * 0.2f);
                }
            }

            if ((damageType.damageType & DamageType.WeakPointHit) != 0)
            {
                damage *= 1.5f;
            }

            if (this.targetBody.HasBuff(RoR2Content.Buffs.DeathMark))
            {
                damage *= 1.5f;
            }

            if ((damageType.damageType & DamageType.BypassArmor) == 0)
            {
                float armor = this.targetBody.armor + this.source.adaptiveArmorValue;

                if ((this.targetBody.bodyFlags & CharacterBody.BodyFlags.ResistantToAOE) != 0 && (damageType & DamageType.AOE) != 0)
                {
                    armor += 300f;
                }

                float armorDamageMultiplier = armor >= 0f ? 1f - (armor / (armor + 100f))
                                                          : 2f - (100f / (100f - armor));

                damage = Mathf.Max(1f, damage * armorDamageMultiplier);

                if (this.targetBody.inventory)
                {
                    itemCount = this.targetBody.inventory.GetItemCount(RoR2Content.Items.ArmorPlate);
                    if (itemCount > 0)
                    {
                        damage = Mathf.Max(1f, damage - (5f * itemCount));
                    }
                }
            }

            if (this.targetBody.hasOneShotProtection && (damageType.damageType & DamageType.BypassOneShotProtection) == 0)
            {
                float unprotectedHealth = (this.source.fullCombinedHealth + this.source.barrier) * (1f - this.targetBody.oneShotProtectionFraction);
                float maxAllowedDamage = Mathf.Max(0f, unprotectedHealth - this.source.serverDamageTakenThisUpdate);

                damage = Mathf.Min(damage, maxAllowedDamage);
            }

            if ((damageType.damageType & DamageType.BonusToLowHealth) != 0)
            {
                damage *= Mathf.Lerp(3f, 1f, this.source.combinedHealthFraction);
            }

            if (this.targetBody.HasBuff(RoR2Content.Buffs.LunarShell))
            {
                damage = Mathf.Min(damage, this.source.fullHealth * 0.1f);
            }

            if (this.targetBody.inventory)
            {
                itemCount = this.targetBody.inventory.GetItemCount(RoR2Content.Items.MinHealthPercentage);
                if (itemCount > 0)
                {
                    float minHealth = this.source.fullCombinedHealth * (itemCount / 100f);
                    damage = Mathf.Max(0f, Mathf.Min(damage, this.source.combinedHealth - minHealth));
                }
            }

            if (this.targetBody.HasBuff(DLC2Content.Buffs.DelayedDamageBuff))
            {
                damage *= 0.8f;
            }
        }

        public void SetCharacterIcon(Color? colorOverride = null)
        {
            this.gunIconHolder.SetActive(false);
            this.characterIconHolder.SetActive(true);

            if (this.targetBody)
            {
                var bodyColor = colorOverride ?? this.targetBody.bodyColor;
                //bodyColor.a = 0.25f;

                this.characterIconBorder.color = bodyColor;
                this.characterIcon.texture = this.targetBody.portraitIcon;
            }
        }

        public void SetGenericIcon(Texture icon)
        {
            this.gunIconHolder.SetActive(true);
            this.characterIconHolder.SetActive(false);

            this.gunIcon.texture = icon;
        }
    }
}
