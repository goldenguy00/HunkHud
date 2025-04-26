namespace HunkHud.Components
{
    public class MoneyDisplayMover : DisplayMover
    {
        private int cachedMoney;
        private int cachedCoin;

        private void Awake()
        {
            this.offset = new UnityEngine.Vector3(-450f, 100f, 0f);
        }

        public override void CheckForActivity()
        {
            base.CheckForActivity();

            if (!this.targetHud)
                return;

            if (this.targetHud.lunarCoinText)
            {
                var coin = this.targetHud.lunarCoinText.displayAmount;
                if (coin != this.cachedCoin)
                {
                    this.cachedCoin = coin;
                    this.activeTimer = this.refreshTimer;
                }
            }
            if (this.targetHud.moneyText)
            {
                var coin = this.targetHud.moneyText.displayAmount;
                if (coin != this.cachedMoney)
                {
                    this.cachedMoney = coin;
                    this.activeTimer = this.refreshTimer;
                }
            }
        }
    }
}
