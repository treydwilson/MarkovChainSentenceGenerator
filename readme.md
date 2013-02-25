Markov Chain Sentence Generator
===============================

Overview
--------
This is an implentation of the Markov Chain algorithm for generating sentences.  The Markov Chain algorithm is simple:  It parses a string and determines the probability that one word will follow another.  There are four main functions that are used:
+  feed(string) - Feeds in a string that is used to create the data for generating new sentences.  The idea is that the sentence generator will generate sentences similar to the sentences fed into it.
+  feed(XmlDocument) - Feeds in an XML document that was previously exported.
+  save(string) - Saves the data stored from inputs into an XML document at the specified path, which can then be imported later using feed(XmlDocument).
+  generateSentence() - Generates a sentence using the data stored.

Classes included
----------------

+  TextMarkovChain - A simple implementation of a single Markov Chain. 
+  DeepMarkovChain - An implementation of the Markov Chain that takes into consideration the two previous words when deciding what word to generate next.  (Note: This implementation does not currently include import and export to XML)
+  MultiDeepMarkovChain - An implementation that will consider the previous X words when deciding what word to generate next, where X is any integer greater than 2.

GUI overview
------------
Included in this code is a WPF application that can be used to test out the package.  The default Markov implementation used is one that used a word storage depth of four.  The GUI allows you to:
+  Enter in sample strings.
+  Generate one sentence at a time.
+  Export the data entered into an XML document.
+  Import the data from an XML document.

Note about Depth
----------------
The depth entered into the MultiDeepMarkovChain class makes a large difference in the storage size required and the sentences that will be generated. Here is a quick overview:
+  Low depth - Sentences will be versatile but are more likely to not make sense.  Data storage required is minimum.
+  Medium depth - Sentences will be more consistent, but more data storage will be required.
+  High depth - Sentences will be very consistent, but will be less flexible.  Oftentimes the exact sentences will be generated as were fed into the program.  Data storage required is much greater.
+  Very high depth - Sentences will generally be exact repeats of sentences that are fed into the program.  Data storage will be very high. Essentially a very high depth just stores sentences is a very inefficient manner.