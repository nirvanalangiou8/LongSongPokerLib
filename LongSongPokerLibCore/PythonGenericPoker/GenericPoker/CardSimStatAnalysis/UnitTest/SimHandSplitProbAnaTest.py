import os
import unittest
from GenericPoker.EightCardRule import EightCardRule
from GenericPoker.NineCardRule import NineCardRule
from GenericPoker.CardSimStatAnalysis.InitEightCardHandSplitProbAna import InitEightCardHandSplitProbAna


EXPECTED_8CARDS_SPLIT_STATS_RESULT = """Hand Position,Rank,Count,Accumulated Count,Probabilities,Win/NoLose Probabilities
Front,FourOfKind,377,3593591176,0.0000104908984227%,100.0000000000000000%
Front,FourCardsFlushStraight,4838,3593590799,0.0001346285585381%,99.9999895091015722%
Front,ThreeCardsFlushStraight,6266009,3593585961,0.1743662173328979%,99.9998548805430376%
Front,ThreeOfKind,8647866,3587319952,0.2406469065751067%,99.8254886632101379%
Front,TwoPairs,21900282,3578672086,0.6094260845880928%,99.5848417566350341%
Front,Pair,930370320,3556771804,25.8897096089708345%,98.9754156720469402%
Front,Nothing,2626401484,2626401484,73.0857060630761057%,73.0857060630761057%
Back,EightCardsFlushStraight,21,3593591178,0.0000005843736519%,100.0000000000000000%
Back,SevenCardsFlushStraight,2757,3593591157,0.0000767199122949%,99.9999994156263505%
Back,EightCardsFlush,20725,3593588400,0.0005767211397579%,99.9999226957140497%
Back,SixCardsFlushStraight,69858,3593567675,0.0019439606939062%,99.9993459745742941%
Back,SevenCardsFlush,1062546,3593497817,0.0295678041092965%,99.9974020138803876%
Back,EightCardsStraight,1830960,3592435271,0.0509507038866623%,99.9678342097710892%
Back,FiveCardsFlushStraight,2312679,3590604311,0.0643556510868082%,99.9168835058844351%
Back,FourOfKind,6843389,3588291632,0.1904331533841494%,99.8525278547976236%
Back,FourCardsFlushStraight,29661450,3581448243,0.8253985645776203%,99.6620947014134728%
Back,SevenCardsStraight,14354277,3551786793,0.3994410128752826%,98.8366961368358554%
Back,SixCardsFlush,20753672,3537432516,0.5775190045838876%,98.4372551239605653%
Back,SixCardsStraight,65679045,3516678844,1.8276715894141701%,97.8597361193766835%
Back,Mansion,76893581,3450999799,2.1397420349522019%,96.0320645299625131%
Back,FullHouse,91254774,3374106218,2.5393755015501656%,93.8923224950103052%
Back,FiveCardsFlush,196836536,3282851444,5.4774326363286725%,91.3529469934601490%
Back,FiveCardsStraight,224750900,3086014908,6.2542144853851817%,85.8755143571314772%
Back,ThreeCardsFlushStraight,226346673,2861264008,6.2986205661260675%,79.6212998717462872%
Back,ThreeOfKind,253128801,2634917335,7.0438953253685890%,73.3226793056202197%
Back,TwoPairs,648950476,2381788534,18.0585504542887659%,66.2787839802516321%
Back,Pair,1535935713,1732838058,42.7409696017458318%,48.2202335259628689%
Back,Nothing,196902345,196902345,5.4792639242170357%,5.4792639242170357%"""

EXPECTED_9CARDS_SPLIT_STATS_RESULT = """Hand Position,Rank,Count,Accumulated Count,Probabilities,Win/NoLose Probabilities
Front,FourOfKind,2720,3434254586,0.0000792020490003%,100.0000000000000000%
Front,FourCardsFlushStraight,34338,3434251866,0.0009998676318285%,99.9999207979510030%
Front,Mansion,90511,3434217528,0.0026355355356873%,99.9989209303191684%
Front,FullHouse,97890,3434127017,0.0028504002120011%,99.9962853947834795%
Front,FiveCardsFlush,143020,3434029127,0.0041645136205985%,99.9934349945714840%
Front,FiveCardsStraight,230418,3433886107,0.0067094035759409%,99.9892704809508870%
Front,ThreeCardsFlushStraight,22753289,3433655689,0.6625393787855889%,99.9825610773749429%
Front,ThreeOfKind,27050557,3410902400,0.7876689488971975%,99.3200216985893558%
Front,TwoPairs,95800950,3383851843,2.7895704177127653%,98.5323527496921536%
Front,Pair,1298946600,3288050893,37.8232471551542659%,95.7427823319793925%
Front,Nothing,1989104293,1989104293,57.9195351768251210%,57.9195351768251210%
Back,NineCardsFlushStraight,3,3434254584,0.0000000873552012%,100.0000000000000000%
Back,NineCardsFlush,1928,3434254581,0.0000561402759418%,99.9999999126447991%
Back,EightCardsFlushStraight,267,3434252653,0.0000077746129027%,99.9999437723688600%
Back,SevenCardsFlushStraight,9972,3434252386,0.0002903686886365%,99.9999359977559510%
Back,EightCardsFlush,135037,3434242414,0.0039320614327525%,99.9996456290673130%
Back,SixCardsFlushStraight,167120,3434107377,0.0048662670722958%,99.9957135676345699%
Back,NineCardsStraight,1074138,3433940257,0.0312771803524511%,99.9908473005622689%
Back,SevenCardsFlush,3514281,3432866119,0.1023302412224428%,99.9595701202098175%
Back,EightCardsStraight,7834539,3429351838,0.2281292434317677%,99.8572398789873805%
Back,FiveCardsFlushStraight,4189012,3421517299,0.1219773286324308%,99.6291106355556066%
Back,FourOfKind,11801406,3417328287,0.3436380650107331%,99.5071333069231789%
Back,FourCardsFlushStraight,43079344,3405526881,1.2544015869034362%,99.1634952419124471%
Back,SevenCardsStraight,33906556,3362447537,0.9873046732752064%,97.9090936550090052%
Back,SixCardsFlush,44243101,3328540981,1.2882883291799662%,96.9217889817338030%
Back,SixCardsStraight,110197038,3284297880,3.2087614736951021%,95.6335006525538378%
Back,Mansion,122450494,3174100842,3.5655625115997513%,92.4247391788587280%
Back,FullHouse,146673024,3051650348,4.2708838384708403%,88.8591766672589767%
Back,FiveCardsFlush,306257661,2904977324,8.9177331938883428%,84.5882928287881364%
Back,FiveCardsStraight,300313882,2598719663,8.7446598571680037%,75.6705596348998033%
Back,ThreeCardsFlushStraight,236290408,2298405781,6.8803987072147701%,66.9258997777318010%
Back,ThreeOfKind,262036582,2062115373,7.6300861101216491%,60.0455010705170267%
Back,TwoPairs,780161346,1800078791,22.7170504375164284%,52.4154149603953790%
Back,Pair,977217606,1019917445,28.4550135145135163%,29.6983645228789506%
Back,Nothing,42699839,42699839,1.2433510083654299%,1.2433510083654299%"""


class SimHandSplitProbAnaTest(unittest.TestCase):
    @staticmethod
    def _load_clean_csv(file_path: str) -> str:
        lines = []
        with open(file_path, "r", encoding="utf-8") as f:
            for line in f:
                stripped = line.strip()
                if not stripped.startswith("#"):
                    lines.append(stripped)
        return "\n".join(lines)

    def test_eight_card_hand_split_prob_ana(self):
        curr_dir = os.path.dirname(os.path.abspath(__file__))
        source_path = os.path.join(curr_dir, "..", "Data", "stats_result_8cards_for_unittest.csv")
        temp_out = os.path.join(curr_dir, "temp_front_back_stats.csv")

        try:
            InitEightCardHandSplitProbAna.run(source_path, temp_out, EightCardRule())
            self.assertTrue(os.path.exists(temp_out), f"File {temp_out} does not exist")
            
            with open(temp_out, "r", encoding="utf-8") as f:
                actual_lines = [l.strip() for l in f if l.strip() and not l.startswith("#")]
            expected_lines = [l.strip() for l in EXPECTED_8CARDS_SPLIT_STATS_RESULT.split("\n") if l.strip() and not l.startswith("#")]

            self.assertEqual(len(actual_lines), len(expected_lines))
            self.assertEqual(actual_lines[0], expected_lines[0])

            for act, exp in zip(actual_lines[1:], expected_lines[1:]):
                act_parts = act.split(",")
                exp_parts = exp.split(",")
                # Position, Rank, Count, Accumulated Count
                self.assertEqual(act_parts[0], exp_parts[0]) # Position
                self.assertEqual(act_parts[1], exp_parts[1]) # Rank
                self.assertEqual(act_parts[2], exp_parts[2]) # Count
                self.assertEqual(act_parts[3], exp_parts[3]) # Acc Count
                # Probabilities check within float precision
                act_prob = float(act_parts[4].rstrip("%"))
                exp_prob = float(exp_parts[4].rstrip("%"))
                self.assertAlmostEqual(act_prob, exp_prob, places=10)
                act_win_prob = float(act_parts[5].rstrip("%"))
                exp_win_prob = float(exp_parts[5].rstrip("%"))
                self.assertAlmostEqual(act_win_prob, exp_win_prob, places=10)
        finally:
            if os.path.exists(temp_out):
                os.remove(temp_out)

    def test_nine_card_hand_split_prob_ana(self):
        curr_dir = os.path.dirname(os.path.abspath(__file__))
        source_path = os.path.join(curr_dir, "..", "Data", "stats_result_9cards_for_unittest.csv")
        temp_out = os.path.join(curr_dir, "temp_front_back_stats_9cards.csv")

        try:
            InitEightCardHandSplitProbAna.run(source_path, temp_out, NineCardRule())
            self.assertTrue(os.path.exists(temp_out), f"File {temp_out} does not exist")

            with open(temp_out, "r", encoding="utf-8") as f:
                actual_lines = [l.strip() for l in f if l.strip() and not l.startswith("#")]
            expected_lines = [l.strip() for l in EXPECTED_9CARDS_SPLIT_STATS_RESULT.split("\n") if l.strip() and not l.startswith("#")]

            self.assertEqual(len(actual_lines), len(expected_lines))
            self.assertEqual(actual_lines[0], expected_lines[0])

            for act, exp in zip(actual_lines[1:], expected_lines[1:]):
                act_parts = act.split(",")
                exp_parts = exp.split(",")
                self.assertEqual(act_parts[0], exp_parts[0])
                self.assertEqual(act_parts[1], exp_parts[1])
                self.assertEqual(act_parts[2], exp_parts[2])
                self.assertEqual(act_parts[3], exp_parts[3])
                act_prob = float(act_parts[4].rstrip("%"))
                exp_prob = float(exp_parts[4].rstrip("%"))
                self.assertAlmostEqual(act_prob, exp_prob, places=10)
                act_win_prob = float(act_parts[5].rstrip("%"))
                exp_win_prob = float(exp_parts[5].rstrip("%"))
                self.assertAlmostEqual(act_win_prob, exp_win_prob, places=10)
        finally:
            if os.path.exists(temp_out):
                os.remove(temp_out)


if __name__ == "__main__":
    unittest.main()
