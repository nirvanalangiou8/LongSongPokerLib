using System.Collections.Generic;
using System.Linq;
using GenericPoker;
using GenericPoker.EightCard;
using GenericPoker.FourCard;
using NUnit.Framework;
using Newtonsoft.Json;
using System.IO;


namespace EightCardsProbTest
{
    public class CardData
    {
        [JsonProperty("data_keys")]
        public List<string> DataKeys { get; set; }

        [JsonProperty("unique_list")]
        public List<List<string>> UniqueList { get; set; }

        [JsonProperty("arranged_list")]
        public List<List<string>> ArrangedList { get; set; }
    }
    
    
    [TestFixture]
    public class FourCardArrangeHandTest
    {
       
        [Test]
        public void Test_Deck_No_Jokers()
        {
            
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UnitTest", "G:\\My Drive\\GameDev\\RiderProjects\\LongSongPokerLib\\UnitTest\\UniqueCases_and_optimal_answers_Golden_no_jokers.json");
            
            if (File.Exists(jsonPath))
            {

                string json = File.ReadAllText(jsonPath);

                CardData cardData = JsonConvert.DeserializeObject<CardData>(json);

                int totalCheckCount = cardData.UniqueList.Count;
                int inCorrectCount = 0;
                foreach (var (cardStrs, cardAnsStrs) in cardData.UniqueList.Zip(cardData.ArrangedList, (A, B) => (A, B)))
                {
                    var ansStrs = string.Join("_", cardAnsStrs);
                    //Debug.Log($"ansStrs = {ansStrs}");
                    //var cardSymbolStrs = cardStrs.Select(s => PokerCard.CardStrToSymbol(s)).ToList();
                    var cardList = cardStrs.Select(s => FourCardPokerCard.CreateInstance(s)).ToList();
                    var fourCardHands = new FourCardHands(cardList);
                    
                    var runStrs = fourCardHands.GetFourCardsStr();
                    if (ansStrs != runStrs)
                    {
                        inCorrectCount++;
                    }
                }
                Assert.That(inCorrectCount, Is.EqualTo(0));
            }
            else
            {
                Assert.Fail($"Test data file not found at path: {jsonPath}");
            }
            
        }
        [Test]
        public void Test_Deck_With_Jokers()
        {
            
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UnitTest", "G:\\My Drive\\GameDev\\RiderProjects\\LongSongPokerLib\\UnitTest\\UniqueCases_and_optimal_answers_Golden_with_jokers.json");
            
            
            
            if (File.Exists(jsonPath))
            {

                string json = File.ReadAllText(jsonPath);

                CardData cardData = JsonConvert.DeserializeObject<CardData>(json);

                int totalCheckCount = cardData.UniqueList.Count;
                int inCorrectCount = 0;
                foreach (var (cardStrs, cardAnsStrs) in cardData.UniqueList.Zip(cardData.ArrangedList, (A, B) => (A, B)))
                {
                    var ansStrs = string.Join("_", cardAnsStrs);
                  
                    //var cardSymbolStrs = cardStrs.Select(s => PokerCard.CardStrToSymbol(s)).ToList();
                    var cardList = cardStrs.Select(s => FourCardPokerCard.CreateInstance(s)).ToList();
                    var fourCardHands = new FourCardHands(cardList);
                    
                    
                    var runStrs = fourCardHands.GetFourCardsStr();
                    if (ansStrs != runStrs)
                    {
                        inCorrectCount++;
                    }
                }
                Assert.That(inCorrectCount, Is.EqualTo(0));
            }
            else
            {
                Assert.Fail($"Test data file not found at path: {jsonPath}");
            }
            
        }
    }
}