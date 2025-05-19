namespace HunkHud.Components
{
    public class HealthBarMover : DisplayMover
    {
        private uint prevLevel;

        public override void CheckForActivity()
        {
            var source = this.targetBody ? this.targetBody.healthComponent : null;
            if (!source)
                return;

            var currentHp = source.health + source.shield;
            var maxHp = source.fullHealth + source.fullShield;
            if (currentHp / maxHp < 0.99f)
                this.SetActive();

            var newLevel = HG.Convert.FloorToUIntClamped(this.targetBody.level);
            if (newLevel != this.prevLevel)
            {
                this.prevLevel = newLevel;
                this.SetActive();
            }
        }
    }
}