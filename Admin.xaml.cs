﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmallStore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Admin : Window
    {
        public Admin()
        {
            InitializeComponent();
        }

        private void RibbonGoToLogin_Click(object sender, RoutedEventArgs e)
        {
            Login dialog = new Login();
            dialog.ShowDialog();
        }

        private void RibbonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
