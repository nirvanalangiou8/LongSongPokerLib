using System;
using System.Collections.Generic;
using System.Linq;
using GenericPoker;

namespace GenericPoker.EightCard
{
	public interface IBaseHandArrangeStrategy
	{
		float CalcHandWinRate(BaseSubBattleHand firstBattleHand, BaseSubBattleHand secondBattleHand)
		{
			return 0.5f;
		}
		
		(BaseSubBattleHand firstBattleHand, BaseSubBattleHand secondBattleHand) ArrangeComps(
			List<PokerCardComponent> comps,
			List<BasePokerCard>? remainingCards = null,
			ICardRule? rule = null);
	}
	
}