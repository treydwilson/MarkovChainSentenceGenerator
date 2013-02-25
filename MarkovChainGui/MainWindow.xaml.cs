using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Microsoft.Win32;

namespace MarkovChainGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TextMarkovChains.MultiDeepMarkovChain multi = new TextMarkovChains.MultiDeepMarkovChain(8);
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnEntry_Click(object sender, RoutedEventArgs e)
        {
            multi.feed(txtEntry.Text);
            txtEntry.Text = string.Empty;
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if(multi.readyToGenerate())
                txtOutput.Text = multi.generateSentence();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = "xml";
            Nullable<bool> saved = sfd.ShowDialog();
            if (saved == true)
            {
                multi.save(sfd.FileName);
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            Nullable<bool> selected = ofd.ShowDialog();
            if (selected == true)
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(ofd.FileName);
                multi.feed(xd);
            }
        }
    }
}
