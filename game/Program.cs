// See https://aka.ms/new-console-template for more information
using System;
using static System.Collections.Specialized.BitVector32;


namespace Game
{

    internal class Program
    {
        private static string input;
        private static string output;
        private static Dictionary<string, string> DualDeck;
        private static Dictionary<string, Deck> Deck2;
        private static Dictionary<string, PlayerHand> GamePlayers = new Dictionary<string, PlayerHand>();
        private static char[] suitChar = string.Join("", Enum.GetNames<Suits>()).ToCharArray();


        static void Main(string[] args)
        {
            try
            {
                ValidateParams(args);
                var some = Enum.GetValues<Suits>();
                SetupDecks();
                string winnerMessage = FindWinner();
                Console.WriteLine(winnerMessage);


            }
            catch (Exception ex)
            {
                //Console.WriteLine("Got Exception");
                Console.WriteLine($"Exception:\n{ex.Message}");
                Environment.Exit(1);
            }
        }

        private static string FindWinner()
        {
            List<string> winnerMessage = new List<string>();
            Dictionary<string, string> finalList = new Dictionary<string, string>();
            int highest = 0;
            foreach (var player in GamePlayers)
            {
                int faceValue, suitscore;
                faceValue = player.Value.RankScore;
                suitscore = player.Value.SuitScore;

                if (faceValue >= highest)
                {
                    if (faceValue == highest)
                    {
                        finalList.Add(player.Key, faceValue.ToString());
                    }
                    else
                    {
                        highest = faceValue;
                        finalList = new Dictionary<string, string>
                        {
                            { player.Key, faceValue.ToString() }
                        };
                    }
                   
                }

            }
            Dictionary<string, string> winnerList = new Dictionary<string, string>();
            

            if (finalList.Count > 1 )
            {
                int highestSuit = 0 ;
                string winnerDetail;

               foreach(var finalist in finalList)
                {
                    int suitScore = GamePlayers[finalist.Key].SuitScore;
                    if (suitScore >= highestSuit)
                    {
                        if (highestSuit == suitScore)
                        {
                            winnerList.Add(finalist.Key, suitScore.ToString());
                        }
                        else
                        {
                            highestSuit = suitScore;
                            winnerList = new Dictionary<string, string>
                            {
                                { finalist.Key, suitScore.ToString() }
                            };
                        }               
                    }
                }

            }
            else
            {
                winnerList = finalList;
            }

            foreach (var winner in winnerList)
            {
                winnerMessage.Add(winner.Key);
            }


            return string.Join(":",string.Join(",",winnerMessage), highest);
        }

        private static void SetupDecks()
        {
            List<string> processingErrorList = new List<string>();
            var inputContents = File.ReadAllLines(input);
            foreach (var playerData in inputContents)
            {
                PlayerHand player = new PlayerHand();
                string name = string.Empty;
                int highest = 0;
                if (playerData.Contains(":") == false)
                {
                    throw new Exception("All player and hand data was not formated correctly. Expected ':' to sperate player from hand");
                }

                string[] playerAndHand = playerData.Split(":");
                name = playerAndHand[0];
                string[] playerHand = playerAndHand[1].Split(",");
                int handCount = playerHand.Count();

                if (handCount != 5)
                {
                    throw new Exception($"{name} has incorrect amount of cards in hand. Found '{handCount}' and excpected 5 cards in hand");
                }


                for (int i = 0; i < 5; i++)
                {
                    int rank, suit;
                    string tempData = playerHand[i];
                    if (DualDeck.ContainsKey(tempData))
                    {
                        if (DualDeck.TryGetValue(tempData, out string value))
                        {
                            throw new Exception($"Only 2 decks of cards are allowed. Card {tempData} was alreayd dealt twice and does not exist anymore ");
                        }
                    }
                    var index = tempData.IndexOfAny(suitChar);

                    string tempRank = tempData.Substring(0, index);
                    string tempSuit = tempData.Substring(index);
                    Deck tempDeck = new Deck();

                    switch (tempRank.ToLowerInvariant())
                    {
                        case "2":
                            tempDeck.FaceValue = Rank.TWO;
                            break;
                        case "3":
                            tempDeck.FaceValue = Rank.THREE;
                            break;
                        case "4":
                            tempDeck.FaceValue = Rank.FOUR;
                            break;
                        case "5":
                            tempDeck.FaceValue = Rank.FIVE;
                            break;
                        case "6":
                            tempDeck.FaceValue = Rank.SIX;
                            break;
                        case "7":
                            tempDeck.FaceValue = Rank.SEVEN;
                            break;
                        case "8":
                            tempDeck.FaceValue = Rank.EIGHT;
                            break;
                        case "9":
                            tempDeck.FaceValue = Rank.NINE;
                            break;
                        case "10":
                            tempDeck.FaceValue = Rank.TEN;
                            break;
                        case "j":
                            tempDeck.FaceValue = Rank.JACK;
                            break;
                        case "a":
                            tempDeck.FaceValue = Rank.ACE;
                            break;
                        case "q":
                            tempDeck.FaceValue = Rank.QUEEN;
                            break;
                        case "k":
                            tempDeck.FaceValue = Rank.KING;
                            break;
                        default:
                            throw new Exception($"Invalid input for card rank. Exceptected value in list of {string.Join(",", Enum.GetValues(typeof(Rank)))}, found {tempRank}");

                    }

                    switch (tempSuit.ToLowerInvariant())
                    {
                        case "h":
                            tempDeck.SuitScore = Suits.H;
                            break;
                        case "s":
                            tempDeck.SuitScore = Suits.S;
                            break;
                        case "c":
                            tempDeck.SuitScore = Suits.C;
                            break;
                        case "d":
                            tempDeck.SuitScore = Suits.D;
                            break;
                        default:
                            throw new Exception($"Invalid input for card suit. Exceptected value in list of {string.Join(",", Enum.GetValues(typeof(Suits)))}, found {tempSuit}");

                    }

                    player.hand.Add(tempDeck);
                    rank = ((int)tempDeck.FaceValue);
                    suit = ((int)tempDeck.SuitScore);
                    if (rank > highest)
                    {
                        highest = rank;
                        player.SuitScore = suit;
                    }
                    player.RankScore += rank;

                }
                GamePlayers.Add(name, player);
            }

        }

        private static void ValidateParams(string[] args)
        {

            if (args.Length >= 4)
            {
                if (args.Length > 4)
                {
                    Console.WriteLine("Found more arguments than required, will try to only parse the needed requirements.");
                    Console.WriteLine("The needed arguments is '--in' & '--out'");
                }
                input = string.Empty;
                output = string.Empty;

                List<string> errorList = new List<string>();


                if (!args.Contains("--in", StringComparer.InvariantCultureIgnoreCase))
                {
                    errorList.Add("Input file not specified. Use '--in' to specify the input file");
                }
                else
                {
                    input = args[Array.FindIndex(args, t => t.Equals("--in", StringComparison.InvariantCultureIgnoreCase)) + 1];
                    ValidateExtensionAndExist(errorList, input);
                }


                if (!args.Contains("--out", StringComparer.InvariantCultureIgnoreCase))
                {
                    errorList.Add("Output file not specified. Use '--out' to specify the output file");
                }
                else
                {
                    output = args[Array.FindIndex(args, t => t.Equals("--out", StringComparison.InvariantCultureIgnoreCase)) + 1];

                    ValidateExtensionAndExist(errorList, output);

                }

                if (errorList.Count > 0)
                {
                    Console.WriteLine("Found errors, unable to proceed");

                    throw new Exception(string.Join("\n", errorList));
                }
            }
            else
            {
                Console.WriteLine($"Invalid input params, expecting --in 'file.txt' --out 'file.txt', received {string.Join(" ", args)} ");
            }
        }

        private static void ValidateExtensionAndExist(List<string> errorList, string file)
        {
            var extension = Path.GetExtension(file);
            if (extension.Equals(".txt") == false)
            {
                errorList.Add($"Found invalid file extension for file {file}. Was expecting '.txt' and got '{extension}'");
            }

            if (File.Exists(file) == false)
            {
                errorList.Add($"{file} does not exist, please correct location passed in.");
            }
        }
    }

    public class Deck
    {
        public Rank FaceValue { get; set; }
        public Suits SuitScore { get; set; }
    }

    public class PlayerHand
    {
        //public string name { get; set; }
        public int RankScore { get; set; }
        public int SuitScore { get; set; }
        public List<Deck> hand { get; set; } = new List<Deck>();
    }

    public class Hand
    {
        public Deck[] cards { get; set; }
    }

    public enum Suits
    {
        H = 1,
        S = 2,
        C = 3,
        D = 4
    }

    public enum Rank
    {
        TWO = 2,
        THREE = 3,
        FOUR = 4,
        FIVE = 5,
        SIX = 6,
        SEVEN = 7,
        EIGHT = 8,
        NINE = 9,
        TEN = 10,
        JACK = 11,
        QUEEN = 12,
        KING = 13,
        ACE = 11
    };


}