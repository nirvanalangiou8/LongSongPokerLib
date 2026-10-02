import unittest
from GenericPoker.CardSimStatAnalysis.SimPokerCard import SimPokerCard
from GenericPoker.CardSimStatAnalysis.SimStatEstimator import SimStatEstimator

EIGHT_CARD_RAW_TEST_DATA = [
    ("2❤️,2♣️,3❤️,3♣️,4♠️,7🔶,7❤️,8♣️", "Pair*3"),
    ("2❤️,2♣️,3❤️,3♣️,5♠️,7🔶,9❤️,J♣️", "Pair*2"),
    ("2❤️,3♣️,4♠️,5🔶,7❤️,9♣️,J♠️,K🔶", "Nothing"),
    ("2❤️,2♣️,3❤️,4♣️,5♠️,7🔶,9❤️,J♣️", "Pair"),
    ("2❤️,3♣️,4❤️,5♣️,6❤️,8♣️,9♠️,J🔶", "FiveCardsStraight"),
    ("2❤️,2♣️,3❤️,3♣️,5❤️,5♣️,7♠️,10🔶", "Pair*3"),
    ("2❤️,2♣️,2♠️,3❤️,3♣️,5♠️,7🔶,9❤️", "ThreeOfKind_Pair"),
    ("2❤️,3❤️,4❤️,5♠️,5♣️,7🔶,8🔶,9🔶", "ThreeCardsFlushStraight*2_Pair"),
    ("2❤️,3♣️,4❤️,5♣️,6❤️,7♣️,9♠️,J🔶", "SixCardsStraight"),
    ("2❤️,2♣️,2♠️,4♣️,5♠️,7🔶,9❤️,J♣️", "ThreeOfKind"),
    ("2❤️,3❤️,4❤️,6♣️,7♠️,9🔶,J❤️,K♣️", "ThreeCardsFlushStraight"),
    ("2❤️,3♣️,4❤️,5♣️,6❤️,7♠️,7♣️,9🔶", "SixCardsStraight,FiveCardsStraight_Pair"),
    ("2❤️,4❤️,6❤️,8❤️,10❤️,J♠️,J♣️,Q🔶", "FiveCardsFlush_Pair"),
    ("2❤️,4❤️,6❤️,8❤️,10❤️,Q❤️,K♣️,A♠️", "SixCardsFlush"),
    ("2❤️,3♣️,4❤️,5♣️,6❤️,7♣️,8❤️,10♠️", "SevenCardsStraight"),
    ("2❤️,2♣️,2♠️,3❤️,3♣️,5♠️,5♣️,7❤️", "ThreeOfKind_Pair*2"),
    ("2❤️,3❤️,4❤️,5❤️,7♣️,9♠️,J🔶,K❤️", "FourCardsFlushStraight,FiveCardsFlush"),
    ("2❤️,3❤️,4❤️,6♠️,7♠️,7♣️,9❤️,9♣️", "ThreeCardsFlushStraight_Pair*2"),
    ("2❤️,3❤️,4❤️,5❤️,7♠️,7♣️,9🔶,10❤️", "FourCardsFlushStraight_Pair,FiveCardsFlush_Pair"),
    ("2❤️,3♣️,4❤️,5♣️,6❤️,7♣️,9♠️,9🔶", "SixCardsStraight_Pair"),
    ("2❤️,3❤️,4❤️,5♠️,5♣️,5🔶,7❤️,9♣️", "ThreeCardsFlushStraight_ThreeOfKind"),
    ("2❤️,2♣️,2♠️,2🔶,3❤️,5♣️,7♠️,9🔶", "FourOfKind"),
    ("2❤️,2♣️,2♠️,3❤️,3♣️,3♠️,5🔶,7❤️", "ThreeOfKind*2"),
    ("2❤️,2♣️,3❤️,3♣️,5❤️,5♣️,7❤️,7♣️", "Pair*4"),
    ("2❤️,3❤️,4❤️,6♣️,7♣️,8♣️,9♠️,J🔶", "ThreeCardsFlushStraight*2"),
    ("2❤️,2♣️,2♠️,2🔶,3❤️,3♣️,5♠️,7🔶", "FourOfKind_Pair"),
    ("2❤️,3❤️,4❤️,5❤️,6❤️,10❤️,J❤️,Q❤️", "EightCardsFlush,FiveCardsFlushStraight_ThreeCardsFlushStraight"),
    ("2❤️,3♣️,4❤️,5♣️,6❤️,7♣️,8❤️,9♣️", "EightCardsStraight"),
    ("2❤️,4❤️,6❤️,8❤️,10❤️,Q❤️,K♠️,K♣️", "SixCardsFlush_Pair"),
    ("2❤️,2♣️,2♠️,3♠️,4♠️,5♠️,6♠️,7♠️", "SixCardsFlushStraight_Pair,FiveCardsFlushStraight_ThreeOfKind"),
    ("2❤️,3❤️,4❤️,6♠️,7♠️,8♠️,9♠️,10♠️", "FiveCardsFlushStraight_ThreeCardsFlushStraight"),
    ("2❤️,4❤️,6❤️,8❤️,10❤️,Q❤️,A❤️,K♣️", "SevenCardsFlush"),
    ("2❤️,3❤️,4❤️,6🔶,8🔶,10🔶,Q🔶,A🔶", "ThreeCardsFlushStraight_FiveCardsFlush"),
    ("2❤️,2♣️,2♠️,4🔶,6🔶,8🔶,10🔶,Q🔶", "ThreeOfKind_FiveCardsFlush"),
    ("2❤️,3❤️,4❤️,5❤️,6❤️,10♠️,10♣️,J🔶", "FiveCardsFlushStraight_Pair"),
    ("2❤️,3❤️,4❤️,5❤️,7♠️,7♣️,9🔶,9♣️", "FourCardsFlushStraight_Pair*2"),
    ("2❤️,3❤️,4❤️,5❤️,7♣️,8♣️,9♣️,10♠️", "FourCardsFlushStraight_ThreeCardsFlushStraight"),
    ("2❤️,2♣️,2♠️,4❤️,4♣️,4♠️,6❤️,6♣️", "ThreeOfKind*2_Pair"),
    ("2❤️,3❤️,4❤️,5❤️,7♠️,7♣️,7🔶,9❤️", "FourCardsFlushStraight_ThreeOfKind,ThreeOfKind_FiveCardsFlush"),
    ("2❤️,3❤️,4❤️,6♠️,6♣️,8♣️,9♣️,10♣️", "ThreeCardsFlushStraight*2_Pair"),
    ("2❤️,3❤️,4❤️,5❤️,6❤️,7❤️,9♣️,10♠️", "SixCardsFlushStraight"),
    ("2❤️,2♣️,2♠️,2🔶,3❤️,3♣️,4❤️,4♣️", "FourOfKind_Pair*2,ThreeCardsFlushStraight*2_Pair,ThreeCardsFlushStraight_ThreeOfKind"),
    ("2❤️,2♣️,2♠️,2🔶,3❤️,3♣️,3♠️,5❤️", "FourOfKind_ThreeOfKind"),
    ("2❤️,2♣️,2♠️,2🔶,3❤️,4❤️,5❤️,7♣️", "FourOfKind_ThreeCardsFlushStraight,FourCardsFlushStraight_ThreeOfKind"),
    ("2❤️,3❤️,4❤️,5❤️,6❤️,7❤️,9♠️,9♣️", "SixCardsFlushStraight_Pair"),
    ("2❤️,3❤️,4❤️,5❤️,6❤️,8♠️,9♠️,10♠️", "FiveCardsFlushStraight_ThreeCardsFlushStraight"),
    ("2❤️,3❤️,4❤️,5❤️,6❤️,8♠️,8♣️,8🔶", "FiveCardsFlushStraight_ThreeOfKind"),
    ("2❤️,3❤️,4❤️,5❤️,6❤️,7❤️,8❤️,10♣️", "SevenCardsFlushStraight"),
    ("2❤️,3❤️,4❤️,5❤️,7♠️,8♠️,9♠️,10♠️", "FourCardsFlushStraight*2"),
    ("2❤️,2♣️,2♠️,2🔶,3❤️,3♣️,3♠️,3🔶", "FourOfKind*2"),
    ("2❤️,3❤️,4❤️,5❤️,6❤️,7❤️,8❤️,9❤️", "EightCardsFlushStraight"),
    ("A❤️,2❤️,3❤️,4❤️,5❤️,K❤️,Q❤️,J❤️", "EightCardsFlush,FiveCardsFlushStraight_ThreeCardsFlushStraight,FourCardsFlushStraight*2"),
    ("A❤️,2♣️,3♣️,4❤️,5♠️,J❤️,K♠️,Q❤️", "FiveCardsStraight"),
]

NINE_CARD_RAW_TEST_DATA = [
    ("7♠️,5❤️,9♠️,4🔶,2♠️,10❤️,8❤️,6❤️,3🔶", "NineCardsStraight"),
]


class SimHandTypeTest(unittest.TestCase):
    def test_eight_card_sim_hand_type(self):
        for cards_str, expected in EIGHT_CARD_RAW_TEST_DATA:
            with self.subTest(cards=cards_str, expected=expected):
                cards = [SimPokerCard.create_instance(s.strip()) for s in cards_str.split(',') if s.strip()]
                calculator = SimStatEstimator()
                calculator.setup_cards(cards)
                results = calculator.test_sim_cards()
                run_str = ",".join(r.final_comps_str for r in results)
                self.assertEqual(run_str, expected)

    def test_nine_card_sim_hand_type(self):
        for cards_str, expected in NINE_CARD_RAW_TEST_DATA:
            with self.subTest(cards=cards_str, expected=expected):
                cards = [SimPokerCard.create_instance(s.strip()) for s in cards_str.split(',') if s.strip()]
                calculator = SimStatEstimator()
                calculator.setup_cards(cards)
                results = calculator.test_sim_cards()
                run_str = ",".join(r.final_comps_str for r in results)
                self.assertEqual(run_str, expected)


if __name__ == "__main__":
    unittest.main()
