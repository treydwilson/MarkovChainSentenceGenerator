using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextMarkovChains
{
    public class DeepMarkovChain
    {
        /* Order of action:
         * Add first word to header chain
         * Add second word to header chain as a nextWord
         * Add third word as nextWord to first word
         * Add fourth word as nextWord to second word
         * etc. etc.
         */

        private Dictionary<string, DoubleChain> chains;
        private DoubleChain head;

        public DeepMarkovChain()
        {
            chains = new Dictionary<string, DoubleChain>();
            head = new DoubleChain() { text = "[]" };
            chains.Add("[]", head);
        }

        public void feed(string s)
        {
            s = s.ToLower();
            s = s.Replace("/", "").Replace(",", "").Replace("[]", "");
            s = s.Replace("\r", "").Replace("\n", "");
            s = s.Replace(".", " .").Replace("!", " !").Replace("?", " ?");
            
            string[] splitValues = s.Split(' ');

            if (splitValues.Length >= 2) //Every input should have a word and punctuation
            {
                addWord("[]", splitValues[0], splitValues[1]);

                for (int i = 0; i < splitValues.Length - 2; i++)
                {
                    if (splitValues[i] == "."
                        || splitValues[i] == "!"
                        || splitValues[i] == "?")
                        addWord("[]", splitValues[i + 1], splitValues[i + 2]);
                    else
                        addWord(splitValues[i], splitValues[i + 1], splitValues[i + 2]);
                }
            }
        }

        private void addWord(string prev, string next, string nextNext)
        {
            if (!chains.ContainsKey(prev))
                return;
            if (!chains.ContainsKey(next))
                chains.Add(next, new DoubleChain() { text = next });
            if (!chains.ContainsKey(nextNext))
                chains.Add(nextNext, new DoubleChain() { text = nextNext });

            chains[prev].addWord(chains[next]);
            chains[prev].addNextWord(chains[next], chains[nextNext]);
        }

        public bool readyToGenerate()
        {
            DoubleChain next = head.getNextWord();
            if (next == null)
                return false;
            return head.getNextNextWord(next) != null;
        }

        public string generateSentence()
        {
            StringBuilder s = new StringBuilder();

            DoubleChain currentString = head.getNextWord();
            DoubleChain nextString = head.getNextNextWord(currentString);
            DoubleChain nextNextString = currentString.getNextNextWord(nextString);
            s.Append(currentString.text);
            s.Append(" ");
            s.Append(nextString.text);

            while (nextNextString.text != "!" && nextNextString.text != "?" && nextNextString.text != ".")
            {
                s.Append(" ");
                s.Append(nextNextString.text);
                currentString = nextString;
                nextString = nextNextString;
                nextNextString = currentString.getNextNextWord(nextString);
                if (nextNextString == null)
                    break;
            }

            s.Append(nextNextString.text); //Add punctuation
            s[0] = char.ToUpper(s[0]);

            return s.ToString();
        }
 
        //Still TODO:  Need to make actual main class  (feed and generate)
        //TODO: Need to add export and import to XML
        //With this implementation I should have an easier time writing decent sentences and doing specific requests
        //In the future I would like to write one that will go n levels deep for each word (This will allow
        //it to be customizable and easier to test the various levels of deepness.  The idea is that if you have
        //a lot of data to work with, deeper chains are better as they will more closely resemble actual sentences.
        //For less data, less depth is good because it allows the computer to improvise a little.
        //I wonder if it would be possible to store multiple sets of data for different amounts of depth?  Then the computer
        //could decide on its own whether to use deep or not deep sets of data.

        private class DoubleChain
        {
            public string text;
            public int fullCount;
            public Dictionary<string, ChainProbability> nextNodes;
            public Dictionary<string, Dictionary<string, ChainProbability>> nextNextNodes;

            public DoubleChain()
            {
                nextNextNodes = new Dictionary<string, Dictionary<string, ChainProbability>>();
                nextNodes = new Dictionary<string, ChainProbability>();
                fullCount = 0;
            }

            public void addWord(DoubleChain c)
            {
                fullCount++;
                if (nextNodes.ContainsKey(c.text))
                    nextNodes[c.text].count++;
                else
                {
                    nextNodes.Add(c.text, new ChainProbability(c, 1));
                    nextNextNodes.Add(c.text, new Dictionary<string, ChainProbability>());
                }
            }

            public void addNextWord(DoubleChain n, DoubleChain nn)
            {
                Dictionary<string, ChainProbability> d = nextNextNodes[n.text];

                if(d.ContainsKey(nn.text))
                    d[nn.text].count++;
                else
                    d.Add(nn.text, new ChainProbability(nn, 1));

                //Add to n as a normal word
                n.addWord(nn);
            }

            public DoubleChain getNextWord()
            {
                int currentCount = RandomHandler.random.Next(fullCount);
                foreach (string key in nextNodes.Keys)
                {
                    for (int i = 0; i < nextNodes[key].count; i++)
                    {
                        if (currentCount == 0)
                            return nextNodes[key].chain;
                        currentCount--;
                    }
                }
                return null;
            }

            public DoubleChain getNextNextWord(DoubleChain c)
            {
                Dictionary<string, ChainProbability> d = nextNextNodes[c.text];
                int fullCount = 0;
                foreach (string key in d.Keys)
                    fullCount += d[key].count;
                int currentCount = RandomHandler.random.Next(fullCount);
                foreach (string key in d.Keys)
                {
                    for (int i = 0; i < d[key].count; i++)
                    {
                        if (currentCount == 0)
                            return d[key].chain;
                        currentCount--;
                    }
                }
                return null;
            }
        }

        private class ChainProbability
        {
            public DoubleChain chain;
            public int count;
            public ChainProbability(DoubleChain c, int co)
            {
                chain = c;
                count = co;
            }
        }
    }

    
}
