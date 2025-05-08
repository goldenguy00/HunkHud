using System;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HunkHud.Components.UI
{
    public class CustomHealthBar : MonoBehaviour
    {
        [Serializable]
        public struct BarInfo
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

        public static CustomHealthBar instance;

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

        public BarInfo healingFill;
        public BarInfo healthFill;
        public BarInfo shieldFill;
        public BarInfo barrierFill;
        public BarInfo barrierFillShiny;
        public BarInfo damageFill;
        public BarInfo curseFill;
        public BarInfo ospFill;
        public BarInfo delayedDamageMask;
        public BarInfo echoFill;
        public BarInfo collapseFill;
        public BarInfo expFill;

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

        public BandDisplayController bandDisplayController;
        public HealthBarMover hpBarMover;

        [NonSerialized]
        public HealthComponent source;

        private CharacterBody _targetBody;

        public CharacterBody targetBody
        {
            get => this._targetBody;
            set
            {
                if (this._targetBody != value)
                {
                    this._targetBody = value;
                    this.source = this._targetBody ? this._targetBody.GetComponent<HealthComponent>() : null;
                    this.updateTimer = 0f;
                }
            }
        }

        private float updateTimer;
        private float updateDelay = 0.1f;
        private float fillSpeed = 1.5f;
        private float minFill = 0.341f;
        private float maxFill = 0.752f;
        private float expFillMin = 0f;
        private float expFillMax = 0.76f;
        private float inverseFillMin = 0.248f;
        private float inverseFillMax = 0.659f;

        private void Awake()
        {
            SingletonHelper.Assign(ref CustomHealthBar.instance, this);

            this.immunityDisplay.SetActive(false);
            this.immunityText.SetActive(false);
            this.biomassBar.SetActive(false);
            this.gunIconHolder.SetActive(false);
            this.characterIconHolder.SetActive(true);
            this.updateTimer = 0f;
        }

        private void OnDestroy()
        {
            SingletonHelper.Unassign(ref CustomHealthBar.instance, this);
        }

        private void Update()
        {
            updateTimer -= Time.deltaTime;
            if (updateTimer <= 0f)
            {
                SetBarFill();
                updateTimer = updateDelay;
            }

            this.ApplyBars();
        }

        private void ApplyBars()
        {
            Apply(ref healthFill);
            Apply(ref healingFill);
            Apply(ref shieldFill);
            Apply(ref barrierFill);
            Apply(ref barrierFillShiny);
            Apply(ref collapseFill);
            Apply(ref echoFill);
            Apply(ref damageFill);
            Apply(ref delayedDamageMask);
            Apply(ref expFill);
            Apply(ref curseFill);
            Apply(ref ospFill);

            void Apply(ref BarInfo info)
            {
                info.image.enabled = info.enabled;

                if (info.currentFill != info.targetFill)
                    info.currentFill = Mathf.Lerp(info.currentFill, info.targetFill, this.fillSpeed * Time.deltaTime);

                if (info.enabled)
                    info.image.fillAmount = info.currentFill;
            }
        }

        private void SetBarFill()
        {
            if (!this.targetBody || !this.source || !this.source.alive)
            {
                this.healingFill.enabled = false;
                this.healthFill.enabled = false;
                this.shieldFill.enabled = false;
                this.barrierFill.enabled = false;
                this.barrierFillShiny.enabled = false;
                this.collapseFill.enabled = false;
                this.echoFill.enabled = false;
                this.delayedDamageMask.enabled = false;
                this.ospFill.enabled = false;
                this.damageFill.targetFill = 0f;
                return;
            }

            HealthComponent.HealthBarValues barInfos = this.source.GetHealthBarValues();


            // health bars

            this.shieldFill.image.color = barInfos.hasVoidShields ? this.pinkShieldColor : this.shieldColor;
            this.healthFill.image.color = barInfos.hasInfusion ? this.infusionColor : this.healthBarGradient.Evaluate(barInfos.healthFraction);

            var healthFillAmount = Util.Remap(barInfos.healthFraction, 0f, 1f, this.minFill, this.maxFill);
            this.healthFill.enabled = barInfos.healthFraction > 0f;
            this.healthFill.targetFill = healthFillAmount;
            this.healthFill.currentFill = Mathf.Min(healthFillAmount, this.healthFill.currentFill);

            this.healingFill.enabled = this.healthFill.currentFill < healthFillAmount;
            this.healingFill.targetFill = healthFillAmount;
            this.healingFill.currentFill = healthFillAmount;

            var currentHealthFillAmount = Util.Remap(barInfos.healthFraction + barInfos.shieldFraction, 0f, 1f, this.minFill, this.maxFill);
            this.damageFill.targetFill = currentHealthFillAmount;
            this.damageFill.currentFill = Mathf.Max(currentHealthFillAmount, this.damageFill.currentFill);
            this.damageFill.enabled = this.damageFill.currentFill > currentHealthFillAmount;

            this.shieldFill.enabled = barInfos.shieldFraction > 0f;
            this.shieldFill.targetFill = currentHealthFillAmount;
            this.shieldFill.currentFill = Mathf.Min(currentHealthFillAmount, this.shieldFill.currentFill);

            var barrierFillAmount = Util.Remap(barInfos.barrierFraction, 0f, 1f, this.minFill, this.maxFill);
            this.barrierFill.enabled = barInfos.barrierFraction > 0f;
            this.barrierFill.targetFill = barrierFillAmount;
            this.barrierFill.currentFill = barrierFillAmount;

            this.barrierFillShiny.enabled = this.barrierFill.enabled;
            this.barrierFillShiny.targetFill = this.barrierFill.targetFill;
            this.barrierFillShiny.currentFill = this.barrierFill.currentFill;

            var curseFillAmount = Util.Remap(barInfos.curseFraction, 0f, 1f, this.inverseFillMin, this.inverseFillMax);
            this.curseFill.enabled = barInfos.curseFraction > 0f;
            this.curseFill.targetFill = curseFillAmount;
            this.curseFill.currentFill = curseFillAmount;

            // inverse fill

            var collapseFraction = this.GetCollapseFraction();
            var collapseFillAmount = Util.Remap(collapseFraction, 0f, 1f, 1f - currentHealthFillAmount, this.inverseFillMax);
            this.collapseFill.enabled = collapseFraction > 0f;
            this.collapseFill.targetFill = collapseFillAmount;
            this.collapseFill.currentFill = collapseFillAmount;

            var echoFillAmount = Util.Remap(barInfos.echoFraction, 0f, 1f, 1f - currentHealthFillAmount, this.inverseFillMax);
            this.echoFill.enabled = barInfos.echoFraction > 0f && this.targetBody.HasBuff(DLC2Content.Buffs.DelayedDamageDebuff);
            this.echoFill.targetFill = echoFillAmount;
            this.echoFill.currentFill = echoFillAmount;

            if (this.echoFill.enabled || this.collapseFill.enabled)
                this.hpBarMover?.SetActive();

            var ospFillAmount = Util.Remap(this.targetBody.oneShotProtectionFraction, 0f, 1f, this.inverseFillMin, this.inverseFillMax);
            this.ospFill.enabled = ospFillAmount > 1f - currentHealthFillAmount;
            this.ospFill.targetFill = this.targetBody.oneShotProtectionFraction;
            this.ospFill.currentFill = this.targetBody.oneShotProtectionFraction;

            this.delayedDamageMask.enabled = this.collapseFill.enabled | this.echoFill.enabled | this.ospFill.enabled;
            this.delayedDamageMask.targetFill = currentHealthFillAmount;
            this.delayedDamageMask.currentFill = currentHealthFillAmount;

            UpdateExpBar();
        }
        
        private void UpdateExpBar()
        {
            var teamIndex = this.targetBody.teamComponent.teamIndex;
            var currentExp = TeamManager.instance.GetTeamCurrentLevelExperience(teamIndex);
            var expFillAmount = Util.Remap(TeamManager.instance.GetTeamExperience(teamIndex) - currentExp, 0f, TeamManager.instance.GetTeamNextLevelExperience(teamIndex) - currentExp, this.expFillMin, this.expFillMax);

            this.expFill.enabled = true;
            this.expFill.targetFill = expFillAmount;
            this.expFill.currentFill = Mathf.Min(expFillAmount, this.expFill.currentFill);

            this.levelText.text = Mathf.RoundToInt(this.targetBody.level).ToString();

            var isImmune = this.targetBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility);
            this.immunityDisplay.SetActive(isImmune);
            this.immunityText.SetActive(isImmune);
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

                if (stackDamage > 0f)
                    collapseDamage += stackDamage;
            }

            float num = 1f - 1f / this.targetBody.cursePenalty;
            float num2 = (1f - num) / this.source.fullCombinedHealth;
            return Mathf.Clamp01(collapseDamage * num2);
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


        public void SetCharacterIcon(CharacterBody body, Color? colorOverride = null)
        {
            this.targetBody = body;
            this.gunIconHolder.SetActive(false);
            this.characterIconHolder.SetActive(true);

            if (body)
            {
                var bodyColor = colorOverride ?? body.bodyColor;
                bodyColor.a = 0.25f;

                this.characterIconBorder.color = bodyColor;
                this.characterIcon.texture = body.portraitIcon;
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
