using IdleStarforge.Data;

namespace IdleStarforge.Economy
{
    public class UpgradeDefinition
    {
        public string Id;
        public string NameRu;
        public string DescriptionRu;
        public ResourceType CostResource;
        public ResourceType TargetResource;
        public double BaseCost;
        public double CostGrowth;
        public double BaseIncomeBonus;
        public double IncomeGrowth;
        public int MaxLevel;
        public int TierRequired;
    }
}
