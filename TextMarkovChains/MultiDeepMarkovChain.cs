using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TextMarkovChains
{
    public class MultiDeepMarkovChain
    {
        private Dictionary<string, Chain> chains;
        private Chain head;
        private int depth;

        /// <summary>
        /// Creates a new multi-deep Markov Chain with the depth passed in
        /// </summary>
        /// <param name="depth">The depth to store information for words.  Higher values mean more consistency but less flexibility.  Minimum value of three.</param>
        public MultiDeepMarkovChain(int depth)
        {
            if (depth < 3)
                throw new ArgumentException("We currently only support Markov Chains 3 or deeper.  Sorry :(");
            chains = new Dictionary<string, Chain>();
            head = new Chain() { text = "[]" };
            chains.Add("[]", head);
            this.depth = depth;
        }

        /// <summary>
        /// Feed in text that wil be used to create predictive text.
        /// </summary>
        /// <param name="s">The text that this Markov chain will use to generate new sentences</param>
        public void feed(string s)
        {
            s = s.ToLower();
            s = s.Replace("/", "").Replace("\\", "").Replace("[]", "").Replace(",", "");
            s = s.Replace("\r\n\r\n", " ").Replace("\r", "").Replace("\n", " "); //The first line is a hack to fix two \r\n (usually a <p> on a website)
            s = s.Replace(".", " .").Replace("!", " ! ").Replace("?", " ?");

            string[] splitValues = s.Split(' ');
            List<string[]> sentences = getSentences(splitValues);
            string[] valuesToAdd;

            foreach (string[] sentence in sentences)
            {
                for (int start = 0; start < sentence.Length - 1; start++)
                {
                    for (int end = 2; end < depth + 2 && end + start <= sentence.Length; end++)
                    {
                        valuesToAdd = new string[end];
                        for (int j = start; j < start + end ; j++)
                            valuesToAdd[j - start] = sentence[j];
                        addWord(valuesToAdd);
                    }
                }
            }
        }

        /// <summary>
        /// Feed in a saved XML document of values that will be used to generate sentences.  Please note that the depth in the XML document must match the depth created by the constructor of this Markov Chain.
        /// </summary>
        /// <param name="xd">The XML document used to load this Markov Chain.</param>
        public void feed(XmlDocument xd)
        {
            XmlNode root = xd.ChildNodes[0];
            int rootDepth = Convert.ToInt32(root.Attributes["Depth"].Value.ToString());
            if (this.depth != rootDepth) //Check to make sure the depths line up
                throw new ArgumentException("The passed in XML document does not have the same depth as this MultiMarkovChain.  The depth of the Markov chain is " + this.depth.ToString() + ", the depth of the XML document is " + rootDepth.ToString() + ".  The Markov Chain depth can be modified in the constructor");

            //First add each word
            foreach (XmlNode xn in root.ChildNodes)
            {
                string text = xn.Attributes["Text"].Value.ToString();
                if(!chains.ContainsKey(text))
                    chains.Add(text, new Chain() { text = text });
            }

            //Now add each next word (Trey:  I do not like this backtracking algorithm.  This could be made better.)
            List<string> nextWords;
            foreach (XmlNode xn in root.ChildNodes)
            {
                string topWord = xn.Attributes["Text"].Value.ToString();
                Queue<XmlNode> toProcess = new Queue<XmlNode>();
                foreach (XmlNode n in xn.ChildNodes)
                    toProcess.Enqueue(n);

                while (toProcess.Count != 0)
                {
                    XmlNode currentNode = toProcess.Dequeue();
                    int currentCount = Convert.ToInt32(currentNode.Attributes["Count"].Value.ToString());
                    nextWords = new List<string>();
                    nextWords.Add(topWord);
                    //nextWords.Add(currentNode.Attributes["Text"].Value.ToString());
                    XmlNode parentTrackingNode = currentNode;
                    while(parentTrackingNode.Attributes["Text"].Value.ToString() != topWord)
                    {
                        nextWords.Insert(1, parentTrackingNode.Attributes["Text"].Value.ToString());
                        parentTrackingNode = parentTrackingNode.ParentNode;
                    }
                    addWord(nextWords.ToArray(), currentCount);

                    foreach (XmlNode n in currentNode.ChildNodes)
                        toProcess.Enqueue(n);
                }
            }
        }

        private List<string[]> getSentences(string[] words)
        {
            List<string[]> sentences = new List<string[]>();
            List<string> currentSentence = new List<string>();
            currentSentence.Add("[]"); //start of sentence
            for (int i = 0; i < words.Length; i++)
            {
                currentSentence.Add(words[i]);
                if (words[i] == "!" || words[i] == "." || words[i] == "?")
                {
                    sentences.Add(currentSentence.ToArray());
                    currentSentence = new List<string>();
                    currentSentence.Add("[]");
                }
            }
            return sentences;
        }

        private void addWord(string[] words, int count = 1)
        {
            //Note:  This only adds the last word in the array. The other words should already be added by this point
            List<Chain> chainsList = new List<Chain>();
            string lastWord = words[words.Length - 1];
            for (int i = 1; i < words.Length - 1; i++)
                chainsList.Add(this.chains[words[i]]);
            if (!this.chains.ContainsKey(lastWord))
                this.chains.Add(lastWord, new Chain() { text = lastWord });
            chainsList.Add(this.chains[lastWord]);
            Chain firstChainInList = chains[words[0]];
            firstChainInList.addWords(chainsList.ToArray(), count);
        }
        
        /// <summary>
        /// Determines if this Markov Chain is ready to begin generating sentences
        /// </summary>
        /// <returns></returns>
        public bool readyToGenerate()
        {
            return (head.getNextWord() != null);
        }

        /// <summary>
        /// Generate a sentence based on the data passed into this Markov Chain.
        /// </summary>
        /// <returns></returns>
        public string generateSentence()
        {
            StringBuilder sb = new StringBuilder();
            string[] currentChains = new string[depth];
            currentChains[0] = head.getNextWord().text;
            sb.Append(currentChains[0]);
            string[] temp;
            bool doneProcessing = false;
            for (int i = 1; i < depth; i++)
            {
                //Generate the first row
                temp = new string[i];
                for (int j = 0; j < i; j++)
                    temp[j] = currentChains[j];
                currentChains[i] = head.getNextWord(temp).text;
                if (currentChains[i] == "."
                    || currentChains[i] == "?"
                    || currentChains[i] == "!")
                {
                    doneProcessing = true;
                    sb.Append(currentChains[i]);
                    break;
                }
                sb.Append(" ");
                sb.Append(currentChains[i]);
            }

            int breakCounter = 0;
            while (!doneProcessing)
            {
                for (int j = 1; j < depth; j++)
                    currentChains[j - 1] = currentChains[j];
                Chain newHead = chains[currentChains[0]];
                temp = new string[depth - 2];
                for (int j = 1; j < depth - 1; j++)
                    temp[j - 1] = currentChains[j];

                currentChains[depth - 1] = newHead.getNextWord(temp).text;
                if (currentChains[depth - 1] == "." ||
                    currentChains[depth - 1] == "?" ||
                    currentChains[depth - 1] == "!")
                {
                    sb.Append(currentChains[depth - 1]);
                    break;
                }
                sb.Append(" ");
                sb.Append(currentChains[depth - 1]);

                breakCounter++;
                if (breakCounter >= 50) //This is still relatively untested software.  Better safe than sorry :)
                    break;
            }


            sb[0] = char.ToUpper(sb[0]);
            return sb.ToString();
        }

        /// <summary>
        /// Save the data contained in this Markov Chain to an XML document.
        /// </summary>
        /// <param name="path">The file path to save to.</param>
        public void save(string path)
        {
            XmlDocument xd = getXmlDocument();
            xd.Save(path);
        }

        /// <summary>
        /// Get the data for this Markov Chain as an XmlDocument object.
        /// </summary>
        /// <returns></returns>
        public XmlDocument getXmlDocument()
        {
            XmlDocument xd = new XmlDocument();
            XmlElement root = xd.CreateElement("Chains");
            root.SetAttribute("Depth", this.depth.ToString());
            xd.AppendChild(root);

            foreach (string key in chains.Keys)
                root.AppendChild(chains[key].getXml(xd));

            return xd;
        }

        private class Chain
        {
            internal string text;
            internal int fullCount;
            internal Dictionary<string, ChainProbability> nextNodes;

            internal Chain()
            {
                nextNodes = new Dictionary<string, ChainProbability>();
                fullCount = 0;
            }

            internal void addWords(Chain[] c, int count=1)
            {
                if (c.Length == 0)
                    throw new ArgumentException("The array of chains passed in is of zero length.");
                if (c.Length == 1)
                {
                    this.fullCount += count;
                    if (!this.nextNodes.ContainsKey(c[0].text))
                        this.nextNodes.Add(c[0].text, new ChainProbability(c[0], count));
                    else
                        this.nextNodes[c[0].text].count += count;
                    return;
                }

                ChainProbability nextChain = nextNodes[c[0].text];
                for (int i = 1; i < c.Length - 1; i++)
                    nextChain = nextChain.getNextNode(c[i].text);
                nextChain.addWord(c[c.Length - 1],count);
            }

            internal Chain getNextWord()
            {
                int currentCount = RandomHandler.random.Next(fullCount) + 1;
                foreach (string key in nextNodes.Keys)
                {
                    currentCount -= nextNodes[key].count;
                    if (currentCount <= 0)
                        return nextNodes[key].chain;
                }
                return null;
            }

            internal Chain getNextWord(string[] words)
            {
                ChainProbability currentChain = nextNodes[words[0]];
                for (int i = 1; i < words.Length; i++)
                    currentChain = currentChain.getNextNode(words[i]);

                int currentCount = RandomHandler.random.Next(currentChain.count) + 1;
                foreach (string key in currentChain.nextNodes.Keys)
                {
                    currentCount -= currentChain.nextNodes[key].count;
                    if (currentCount <= 0)
                        return currentChain.nextNodes[key].chain;
                }
                return null;
            }

            internal XmlElement getXml(XmlDocument xd)
            {
                XmlElement e = xd.CreateElement("Chain");
                e.SetAttribute("Text", this.text);

                foreach (string key in nextNodes.Keys)
                    e.AppendChild(nextNodes[key].getXML(xd));

                return e;
            }
        }

        private class ChainProbability
        {
            internal Chain chain;
            internal int count;
            internal Dictionary<string, ChainProbability> nextNodes;

            internal ChainProbability(Chain c, int co)
            {
                chain = c;
                count = co;
                nextNodes = new Dictionary<string, ChainProbability>();
            }

            internal void addWord(Chain c, int count = 1)
            {
                string word = c.text;
                if (this.nextNodes.ContainsKey(word))
                    this.nextNodes[word].count += count;
                else
                    this.nextNodes.Add(word, new ChainProbability(c, count));
            }

            internal ChainProbability getNextNode(string prev)
            {
                return nextNodes[prev];
            }

            internal XmlElement getXML(XmlDocument xd)
            {
                XmlElement e = xd.CreateElement("Chain");
                e.SetAttribute("Text", chain.text);
                e.SetAttribute("Count", count.ToString());

                XmlElement c;
                foreach (string key in nextNodes.Keys)
                {
                    c = nextNodes[key].getXML(xd);
                    e.AppendChild(c);
                }

                return e;
            }
        }
    }
}
