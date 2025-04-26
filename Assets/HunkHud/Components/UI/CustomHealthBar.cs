using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HunkHud.Modules;

namespace HunkHud.Components.UI
{
    public class CustomHealthBar : MonoBehaviour
    {
        public static CustomHealthBar instance;

        public Gradient healthBarGradient = new Gradient
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
            new GradientColorKey[]
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

        public CharacterBody targetBody;

        public Color shieldColor = new Color(0.14901961f, 0.70980394f, 0.80784315f);
        public Color pinkShieldColor = new Color(0.80784315f, 0.29803923f, 0.7607843f);
        public Color infusionColor = new Color(1f, 18f / 85f, 0.23137255f);

        public float minFill = 0.341f;
        public float maxFill = 0.752f;
        public float expFillMin = 0f;
        public float expFillMax = 0.76f;
        public float inverseFillMin = 0.248f;
        public float inverseFillMax = 0.659f;
        public float fillSpeed = 1f;

        public Image healingFill;
        public Image healthFill;
        public Image shieldFill;
        public Image barrierFill;
        public Image barrierFillShiny;
        public Image damageFill;
        public Image curseFill;
        public Image collapseFill;
        public Image expFill;

        public TextMeshProUGUI gunText;
        public RawImage gunIcon;
        public GameObject gunIconHolder;

        public RawImage characterIcon;
        public RawImage characterIconBorder;
        public GameObject characterIconHolder;

        public GameObject immunityDisplay;
        public GameObject immunityText;

        public TextMeshProUGUI levelDisplay;
        public TextMeshProUGUI levelText;

        public GameObject biomassBar;

        private void OnEnable()
        {
            SingletonHelper.Assign(ref CustomHealthBar.instance, this);
        }

        private void OnDisable()
        {
            SingletonHelper.Unassign(ref CustomHealthBar.instance, this);
        }

        private void FixedUpdate()
        {
            if (!this.targetBody || !this.targetBody.healthComponent)
                return;

            if (!this.targetBody.healthComponent.alive)
            {
                this.healingFill.fillAmount = 0f;
                this.healthFill.fillAmount = 0f;
                this.shieldFill.fillAmount = 0f;
                this.barrierFill.fillAmount = 0f;
                this.barrierFillShiny.fillAmount = 0f;
                this.collapseFill.fillAmount = 0f;
                this.damageFill.fillAmount = Mathf.Lerp(this.damageFill.fillAmount, 0f, this.fillSpeed * Time.fixedDeltaTime);
                return;
            }

            this.SetBarFill(this.targetBody.healthComponent);
        }


        public void SetCharacterIcon(Color? colorOverride = null)
        {
            this.gunIconHolder.SetActive(false);
            this.characterIconHolder.SetActive(true);

            if (this.targetBody)
            {
                var bodyColor = colorOverride ?? this.targetBody.bodyColor;
                bodyColor.a = 0.25f;

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

        private void SetBarFill(HealthComponent health)
        {
            HealthComponent.HealthBarValues barInfos = health.GetHealthBarValues();

            this.shieldFill.color = barInfos.hasVoidShields ? this.pinkShieldColor : this.shieldColor;
            this.healthFill.color = barInfos.hasInfusion ? this.infusionColor : this.healthBarGradient.Evaluate(barInfos.healthFraction);

            var healthAmount = Util.Remap(barInfos.healthFraction, 0f, 1f, this.minFill, this.maxFill);
            this.healthFill.fillAmount = healthAmount < this.healthFill.fillAmount ? healthAmount : Mathf.Lerp(this.healthFill.fillAmount, healthAmount, this.fillSpeed * Time.fixedDeltaTime);
            this.healingFill.fillAmount = healthAmount;

            var currentHealth = Util.Remap(barInfos.healthFraction + barInfos.shieldFraction, 0f, 1f, this.minFill, this.maxFill);
            this.damageFill.fillAmount = currentHealth > this.damageFill.fillAmount ? currentHealth : Mathf.Lerp(this.damageFill.fillAmount, currentHealth, this.fillSpeed * Time.fixedDeltaTime);
            this.shieldFill.fillAmount = currentHealth;

            var barrierAmount = Util.Remap(barInfos.barrierFraction, 0f, 1f, this.minFill, this.maxFill);
            this.barrierFill.fillAmount = barrierAmount;
            this.barrierFillShiny.fillAmount = barrierAmount;

            this.curseFill.fillAmount = Util.Remap(barInfos.curseFraction, 0f, 1f, this.inverseFillMin, this.inverseFillMax);

            var collapseAmount = this.GetCollapseValue();
            this.collapseFill.fillAmount = Util.Remap(collapseAmount, 0f, 1f, this.inverseFillMin, this.inverseFillMax);

            var teamIndex = this.targetBody.teamComponent.teamIndex;
            var currentExp = TeamManager.instance.GetTeamCurrentLevelExperience(teamIndex);
            var expAmount = Util.Remap(TeamManager.instance.GetTeamExperience(teamIndex) - currentExp, 0f, TeamManager.instance.GetTeamNextLevelExperience(teamIndex) - currentExp, this.expFillMin, this.expFillMax);
            this.expFill.fillAmount = expAmount < this.expFill.fillAmount ? expAmount : Mathf.Lerp(this.expFill.fillAmount, expAmount, this.fillSpeed * Time.fixedDeltaTime);
            this.levelText.text = Mathf.RoundToInt(this.targetBody.level).ToString();

            var isImmune = this.targetBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility);
            this.immunityDisplay.SetActive(isImmune);
            this.immunityText.SetActive(isImmune);
        }
        private void SetBarFillOld(HealthComponent health)
        {
            var fullCombinedHealth = health.fullCombinedHealth;
            var fullCombinedHealthPercent = health.fullHealth / fullCombinedHealth;
            var healthPercent = health.health / health.fullHealth;

            this.shieldFill.color = this.shieldColor;
            this.healthFill.color = this.healthBarGradient.Evaluate(healthPercent);

            if (this.targetBody.inventory)
            {
                if (this.targetBody.inventory.GetItemCount(DLC1Content.Items.MissileVoid) > 0)
                    this.shieldFill.color = this.pinkShieldColor;

                if (this.targetBody.inventory.GetItemCount(RoR2Content.Items.Infusion) > 0)
                    this.healthFill.color = this.infusionColor;
            }

            var healthAmount = Util.Remap(fullCombinedHealthPercent * healthPercent / this.targetBody.cursePenalty, 0f, 1f, this.minFill, this.maxFill);
            this.healthFill.fillAmount = healthAmount < this.healthFill.fillAmount ? healthAmount : Mathf.Lerp(this.healthFill.fillAmount, healthAmount, this.fillSpeed * Time.fixedDeltaTime);
            this.healingFill.fillAmount = healthAmount;

            var shieldAmount = 0f;
            if (health.fullShield > 0f)
                shieldAmount = health.shield / health.fullShield;

            var currentHealth = Util.Remap(((fullCombinedHealthPercent * healthPercent) + ((1f - fullCombinedHealthPercent) * shieldAmount)) / this.targetBody.cursePenalty, 0f, 1f, this.minFill, this.maxFill);
            this.damageFill.fillAmount = currentHealth > this.damageFill.fillAmount ? currentHealth : Mathf.Lerp(this.damageFill.fillAmount, currentHealth, this.fillSpeed * Time.fixedDeltaTime);
            this.shieldFill.fillAmount = currentHealth;

            var barrierAmount = Util.Remap(health.barrier, 0f, health.fullBarrier, this.minFill, this.maxFill);
            this.barrierFill.fillAmount = barrierAmount;
            this.barrierFillShiny.fillAmount = barrierAmount;

            var curseAmount = Mathf.Clamp(Util.Remap(fullCombinedHealth / this.targetBody.cursePenalty, fullCombinedHealth, 1f, 0f, 1f), 0f, 1f);
            this.curseFill.fillAmount = Util.Remap(curseAmount, 0f, 1f, this.inverseFillMin, this.inverseFillMax);

            var collapseAmount = this.GetCollapseValue();
            this.collapseFill.fillAmount = Util.Remap(collapseAmount, 0f, 1f, this.inverseFillMin, this.inverseFillMax);

            var teamIndex = this.targetBody.teamComponent.teamIndex;
            var currentExp = TeamManager.instance.GetTeamCurrentLevelExperience(teamIndex);
            var expAmount = Util.Remap(TeamManager.instance.GetTeamExperience(teamIndex) - currentExp, 0f, TeamManager.instance.GetTeamNextLevelExperience(teamIndex) - currentExp, this.expFillMin, this.expFillMax);
            this.expFill.fillAmount = expAmount < this.expFill.fillAmount ? expAmount : Mathf.Lerp(this.expFill.fillAmount, expAmount, this.fillSpeed * Time.fixedDeltaTime);
            this.levelText.text = Mathf.RoundToInt(this.targetBody.level).ToString();

            var isImmune = this.targetBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility);
            this.immunityDisplay.SetActive(isImmune);
            this.immunityText.SetActive(isImmune);
        }

        private float GetCollapseValue()
        {
            var dotController = DotController.FindDotController(gameObject);
            if (!dotController || !dotController.HasDotActive(DotController.DotIndex.Fracture))
                return 0f;

            float collapseDamage = 0f;

            foreach (DotController.DotStack dotStack in dotController.dotStackList)
            {
                if (dotStack.dotIndex != DotController.DotIndex.Fracture)
                    continue;

                float stackDamage = dotStack.damage;
                ModifyIncomingDamage(ref stackDamage, dotStack.attackerObject, dotStack.damageType | DamageType.DoT);

                if (stackDamage > 0f)
                    collapseDamage += stackDamage;
            }

            return collapseDamage;
        }

        private void ModifyIncomingDamage(ref float damage, GameObject attacker, DamageTypeCombo damageType)
        {
            int itemCount;
            var attackerBody = attacker ? attacker.GetComponent<CharacterBody>()  : null;

            if ((damageType & DamageTypeExtended.DamagePercentOfMaxHealth) != 0)
            {
                damage = this.targetBody.healthComponent.fullHealth * 0.1f;
            }

            if (this.targetBody.HasBuff(DLC2Content.Buffs.KnockUpHitEnemiesJuggleCount))
            {
                damage *= 1f + (0.1f * this.targetBody.GetBuffCount(DLC2Content.Buffs.KnockUpHitEnemiesJuggleCount));
            }

            if (this.targetBody.HasBuff(DLC2Content.Buffs.lunarruin))
            {
                damage *= 1f + (0.1f * this.targetBody.GetBuffCount(DLC2Content.Buffs.lunarruin));
            }

            if (attackerBody)
            {
                if (attackerBody.teamComponent.teamIndex == this.targetBody.teamComponent.teamIndex)
                {
                    damage *= TeamCatalog.GetTeamDef(attackerBody.teamComponent.teamIndex)?.friendlyFireScaling ?? 1f;
                }

                if (attackerBody.inventory)
                {
                    if (this.targetBody.healthComponent.combinedHealth >= this.targetBody.healthComponent.fullCombinedHealth * 0.9f)
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
            }

            if ((damageType & DamageType.WeakPointHit) != 0)
            {
                damage *= 1.5f;
            }

            if (this.targetBody.HasBuff(RoR2Content.Buffs.DeathMark))
            {
                damage *= 1.5f;
            }

            if ((damageType & DamageType.BypassArmor) == 0)
            {
                float armor = this.targetBody.armor + this.targetBody.healthComponent.adaptiveArmorValue;

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

            if (this.targetBody.hasOneShotProtection && (damageType & DamageType.BypassOneShotProtection) == 0)
            {
                float unprotectedHealth = (this.targetBody.healthComponent.fullCombinedHealth + this.targetBody.healthComponent.barrier) * (1f - this.targetBody.oneShotProtectionFraction);
                float maxAllowedDamage = Mathf.Max(0f, unprotectedHealth - this.targetBody.healthComponent.serverDamageTakenThisUpdate);

                damage = Mathf.Min(damage, maxAllowedDamage);
            }

            if ((damageType & DamageType.BonusToLowHealth) != 0)
            {
                damage *= Mathf.Lerp(3f, 1f, this.targetBody.healthComponent.combinedHealthFraction);
            }

            if (this.targetBody.HasBuff(RoR2Content.Buffs.LunarShell))
            {
                damage = Mathf.Min(damage, this.targetBody.healthComponent.fullHealth * 0.1f);
            }

            if (this.targetBody.inventory)
            {
                itemCount = this.targetBody.inventory.GetItemCount(RoR2Content.Items.MinHealthPercentage);
                if (itemCount > 0)
                {
                    float minHealth = this.targetBody.healthComponent.fullCombinedHealth * (itemCount / 100f);
                    damage = Mathf.Max(0f, Mathf.Min(damage, this.targetBody.healthComponent.combinedHealth - minHealth));
                }
            }

            if (this.targetBody.HasBuff(DLC2Content.Buffs.DelayedDamageBuff))
            {
                damage *= 0.8f;
            }
        }
    }

}
