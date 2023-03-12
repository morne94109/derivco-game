// See https://aka.ms/new-console-template for more information
using System;
//using static System.Collections.Specialized.BitVector32;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Game
{

    internal class game
    {
        //Keep track of used cards.
        private static Dictionary<string, string> DualDeck = new Dictionary<string, string>();
        //Keep track of players in game
        private static Dictionary<string, PlayerHand> GamePlayers = new Dictionary<string, PlayerHand>();

        private static List<string> suitStringList = Enum.GetNames<Suits>().ToList();
        private static string suitString = string.Join(",", suitStringList);      

        private static string rankString = "2,3,4,5,6,7,8,9,10,j,q,k,a";
        private static List<string> rankStringList = rankString.Split(',').ToList();


        static void Main(string[] args)
        {
            string message = string.Empty;
            OptionParams optionParams = new OptionParams();

            try
            {
                //Validate input parameters
                optionParams = ValidateParams(args);

                //Validate input data from input file
                ValidateInput(optionParams.Input);

                //Find the winner and return message to display
                message = FindWinner();

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Got Exception");
                message = ($"Exception:{ex.Message}");
            }
           
            Console.WriteLine(message);

            try
            {

                if (string.IsNullOrEmpty(optionParams.Output))
                {
                    File.WriteAllText("error.txt", message);                    
                }
                else
                {
                    File.WriteAllText(optionParams.Output, message);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception:{ex.ToString()}");
            }
        }

        /// <summary>
        /// Find the winner after score are calculated
        /// </summary>
        /// <returns></returns>
        private static string FindWinner()
        {
            List<string> winnerMessage = new List<string>();
            Dictionary<string, string> finalList = new Dictionary<string, string>();
            int highest = 0;
            
            //Loop through player list to find the highest scores
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
            int highestSuit = 0;

            //If finalist has more than 1 players, loop through list to deter,ome the suit score of the highest card 
            if (finalList.Count > 1)
            {
                foreach (var finalist in finalList)
                {
                    int suitScore = GamePlayers[finalist.Key].SuitScore;
                    if (suitScore >= highestSuit)
                    {
                        if (highestSuit == suitScore)
                        {
                            winnerList.Add(finalist.Key, (highest + suitScore).ToString());
                        }
                        else
                        {
                            highestSuit = suitScore;
                            winnerList = new Dictionary<string, string>
                            {
                                { finalist.Key, (highest + suitScore).ToString() }
                            };
                        }
                    }
                }

                //Calcalate tie break score. (faceValue + highest card suit score)
                if (winnerList.Count == 1)
                {
                    highest += highestSuit;
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

            return string.Join(":", string.Join(",", winnerMessage), highest);

        }

        /// <summary>
        /// Validate input content from inputParam that contains the file location
        /// </summary>
        /// <param name="inputParam"></param>
        /// <exception cref="Exception"></exception>
        private static void ValidateInput(string inputParam)
        {
            string inputFile = inputParam;
            List<string> processingErrorList = new List<string>();
            var inputContents = File.ReadAllLines(inputParam.Trim()).ToList();

            //Filter possible spaces from list
            var contents = inputContents
                .Where(x =>string.IsNullOrWhiteSpace(x) == false)
                .Select(z => z.Trim())
                .ToList();


            if (contents.Count != 7)
            {
                throw new Exception($"Invalid number of players found. Found {contents.Count} and expected 7.");
            }

            foreach (var playerData in contents)
            {
                if (string.IsNullOrWhiteSpace(playerData))
                {
                    continue;
                }

                string localPlayerData = playerData.Trim();


                if (localPlayerData.Contains(":") == false)
                {
                    throw new Exception("All player and hand data was not formated correctly. Expected ':' to sperate player from hand");
                }

                string[] playerAndHand = localPlayerData.Split(":");
                if (playerAndHand.Length != 2)
                {
                    throw new Exception($"Invalid player name and hand. Expected 'Player:Hand', but receivd {localPlayerData}");
                }

                string name = playerAndHand[0].Trim();
                if (GamePlayers.ContainsKey(name))
                {
                    throw new Exception($"Duplicate players found. Player name = {name}");
                }

                string[] playerHand = playerAndHand[1].Split(",");
                               
                int handCount = playerHand.Count();

                if (handCount != 5)
                {
                    throw new Exception($"{name} has incorrect amount of cards in hand. Found '{handCount}' and excpected 5 cards in hand");
                }

                PlayerHand player = ValidateHand(name, playerHand);

                GamePlayers.Add(name, player);
            }

        }

        /// <summary>
        /// Validate playerhand and assign faceValue and suit score
        /// </summary>
        /// <param name="name"></param>
        /// <param name="playerHand"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static PlayerHand ValidateHand(string name, string[] playerHand)
        {
            PlayerHand player = new PlayerHand();
            int highest = 0;

            //Loop through each hand dealt
            for (int i = 0; i < 5; i++)
            {
                int rank, suit;
                string hand = playerHand[i].Trim().ToUpperInvariant();

                if (hand.Length is < 2 or > 3)
                {
                    throw new Exception($"Invalid hand found for player {name}. Expected 'Rank,Suit'(8D) got '{hand}'");
                }

                if (ValidateRank(hand, out string tempRank) == false)
                {
                    throw new Exception($"Invalid rank found for player {name}. Expected 'Rank({rankString})' got '{hand}'");
                }

                if (ValidateSuit(hand, out string tempSuit) == false)
                {
                    throw new Exception($"Invalid suit found for player {name}. Expected 'Suit({suitString})' got '{hand}'");
                }

                if(IsCardAvailible(hand) == false)
                {
                    throw new Exception($"Only 2 decks of cards are allowed. Card {hand} was already dealt twice and does not exist anymore ");
                }

                Deck deck = new Deck();

                //Parse scores
                deck.FaceValue = ParseFaceValue(tempRank);
                deck.SuitScore = ParseSuit(tempSuit);

                player.hand.Add(deck);
                
                //Get scores
                rank = ((int)deck.FaceValue);
                suit = ((int)deck.SuitScore);

                if (rank > highest)
                {
                    highest = rank;
                    player.SuitScore = suit;
                }
                player.RankScore += rank;

            }

            return player;
        }

        /// <summary>
        /// Validate the rank that was dealt the player and return the rank as out parameter if possitive match was found.
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        private static bool ValidateRank(string hand, out string rank)
        {
            bool rankFound = false;
            rank = string.Empty;
            foreach (var item in rankStringList)
            {
                if (hand.StartsWith(item, StringComparison.InvariantCultureIgnoreCase))
                {
                    rankFound = true;
                    rank = item;
                    return rankFound;
                }
            }
            return rankFound;
        }

        /// <summary>
        /// Validate the suit that was dealt the player and return the suit as out parameter if possitive match was found.
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="suit"></param>
        /// <returns></returns>
        private static bool ValidateSuit(string hand, out string suit)
        {
            bool suitFound = false;
            suit = string.Empty;
            foreach (var item in suitStringList)
            {
                if (hand.EndsWith(item, StringComparison.InvariantCultureIgnoreCase))
                {
                    suitFound = true;
                    suit = item;
                    return suitFound;
                }
            }
            return suitFound;
        }

        /// <summary>
        /// Used to check if only 2 Decks are used. Will return true if more than 2 decks are tried being used.
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        private static bool IsCardAvailible(string hand)
        {
            if (DualDeck.ContainsKey(hand))
            {
                if (DualDeck.TryGetValue(hand, out string value) && value.Equals(hand, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false; 
                }
                else
                {
                    DualDeck[hand] = hand;
                }
            }
            else
            {
                DualDeck.Add(hand, string.Empty);
            }
            return true;
        }

        /// <summary>
        /// Get Rank enum from input param
        /// </summary>
        /// <param name="rank"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        private static Rank ParseFaceValue(string rank)
        {
            string value = rank.ToLowerInvariant();
            switch (value)
            {
                case "j":
                    return Rank.JACK;
                case "a":
                    return Rank.ACE;
                case "q":
                    return Rank.QUEEN;
                case "k":
                    return Rank.KING;
                default:
                    if (Int32.TryParse(value, out int num))
                    {
                        if (num >= 2 && num <= 10)
                        {
                            return (Rank)num;
                        }
                        else
                        {
                            throw new ArgumentException("Invalid input file format.");
                        }
                    }
                    throw new Exception($"Invalid input for card rank. Exceptected value in list of {rankString}, found {rank}");

            }
        }

        /// <summary>
        /// Get Suits enum from input param
        /// </summary>
        /// <param name="suit"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static Suits ParseSuit(string suit)
        {
            string value = suit.ToLowerInvariant();
            switch (value)
            {
                case "h":
                    return Suits.H;
                case "s":
                    return Suits.S;
                case "c":
                    return Suits.C;
                case "d":
                    return Suits.D;
                default:
                    throw new Exception($"Invalid input for card suit. Exceptected value in list of {suitString}, found {value}");

            }
        }

        private static OptionParams ValidateParams(string[] args)
        {
            OptionParams optionParams = new OptionParams();
            if (args.Length >= 4)
            {
                if (args.Length > 4)
                {
                    Console.WriteLine("Found more arguments than required, will try to only parse the needed requirements.");
                    Console.WriteLine("The needed arguments is '--in' & '--out'");
                }
                optionParams.Input = string.Empty;
                optionParams.Output = string.Empty;

                List<string> errorList = new List<string>();


                if (!args.Contains("--in", StringComparer.InvariantCultureIgnoreCase))
                {
                    errorList.Add("optionParams.input file not specified. Use '--in' to specify the optionParams.input file");
                }
                else
                {
                    optionParams.Input = args[Array.FindIndex(args, t => t.Equals("--in", StringComparison.InvariantCultureIgnoreCase)) + 1];
                    optionParams.Input = optionParams.Input.Trim();
                    ValidateExtensionAndExist(errorList, optionParams.Input);
                }


                if (!args.Contains("--out", StringComparer.InvariantCultureIgnoreCase))
                {
                    errorList.Add("optionParams.output file not specified. Use '--out' to specify the optionParams.output file");
                }
                else
                {
                    optionParams.Output = args[Array.FindIndex(args, t => t.Equals("--out", StringComparison.InvariantCultureIgnoreCase)) + 1];

                    ValidateExtensionAndExist(errorList, optionParams.Output, false);
                }

                if (errorList.Count > 0)
                {
                    Console.WriteLine("Found errors, unable to proceed");

                    throw new Exception(string.Join("\n", errorList));
                }
            }
            else
            {
                throw new Exception($"Invalid optionParams.input params, expecting --in 'file.txt' --out 'file.txt', received {string.Join(" ", args)} ");
            }

            return optionParams;
        }


        /// <summary>
        /// Used to validate if file is correct and exists
        /// </summary>
        /// <param name="errorList"></param>
        /// <param name="file"></param>
        private static void ValidateExtensionAndExist(List<string> errorList, string file, bool needsExist = true)
        {
            var extension = Path.GetExtension(file);
            if (extension.Equals(".txt") == false)
            {
                errorList.Add($"Found invalid file extension for file {file}. Was expecting '.txt' and got '{extension}'");
            }

            if (needsExist && File.Exists(file) == false)
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

    /// <summary>
    /// Suits enum to easily find related score value
    /// </summary>
    public enum Suits
    {
        H = 1,
        S = 2,
        C = 3,
        D = 4
    }

    /// Rank (faceValue) enum to easily find related score value
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

    /// <summary>
    /// OptionsParam model for input and output parameters
    /// </summary>
    public class OptionParams
    {
        public string Input { get; set; }
        public string Output { get; set; }
    }
}