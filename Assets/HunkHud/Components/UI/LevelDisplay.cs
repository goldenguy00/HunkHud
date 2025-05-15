using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HunkHud
{
    public class LevelDisplay : ExpBar
    {
        private float expFillMin = 0f;
        private float expFillMax = 0.76f;
        
        public Image fillImage;

        private new void Update()
        {
            TeamIndex teamIndex = source ? source.teamIndex : TeamIndex.Player;
            float x = 0f;
            if (TeamManager.instance)
            {
                x = Mathf.InverseLerp(TeamManager.instance.GetTeamCurrentLevelExperience(teamIndex), TeamManager.instance.GetTeamNextLevelExperience(teamIndex), TeamManager.instance.GetTeamExperience(teamIndex));
            }

            if (fillImage)
            {
                this.fillImage.fillAmount = Util.Remap(x, 0f, 1f, expFillMin, expFillMax);
            }
        }
    }
}
