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

		float CalcHandWinRate(IReadOnlyList<BaseSubBattleHand> subHands)
		{
			if (subHands == null || subHands.Count == 0) return 0f;
			if (subHands.Count == 2) return CalcHandWinRate(subHands[0], subHands[1]);
			return 0.5f;
		}
		
		IReadOnlyList<BaseSubBattleHand> ArrangeComps(
			List<PokerCardComponent> comps,
			List<BasePokerCard>? remainingCards = null,
			ICardRule? rule = null);
	}
}