from typing import Optional, Iterable, Any
from GenericPoker.ICardRule import ICardRule
from GenericPoker.EightCardRule import EightCardRule
from GenericPoker.CardSimStatAnalysis.SimCardEnum import SimCardOverAllHandRank


class AssemblyComponent:
    DefaultRule: ICardRule = EightCardRule.default()

    @classmethod
    def assemble_hand_rank(cls, components_or_types: Optional[Iterable[Any]] = None, rule: Optional[ICardRule] = None) -> SimCardOverAllHandRank:
        effective_rule = rule if rule is not None else cls.DefaultRule
        return effective_rule.assemble_hand_rank(components_or_types)
