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
    ///
    enum PaymentMethod
    {
        Select_Method,
        Cash,
        CreditCard,
        DebitCard,
        Cheque
    }
    public partial class Payment : Window
    {
        const decimal TAX_RATE = 0.5M;
        List<OrderItem> orderItems;
        int cashierID;
            int customerId;
        public Payment(List<OrderItem> items, int customerId, int employeeId, decimal totalDiscount,int cashID,int custId)
        {
            InitializeComponent();
            cashierID = cashID;
            customerId = custId;

            foreach (PaymentMethod p in Enum.GetValues(typeof(PaymentMethod)))
            {
                cmbPaymenthMethod.Items.Add(p);
            }
            cmbPaymenthMethod.SelectedIndex = 0;
           
            orderItems = items;
            dgOrders.ItemsSource = orderItems;
            lblTotal_Price.Content = TotalPrice() + totalDiscount + "";
            lblTotalTax.Content = TotalPrice() * TAX_RATE;
            lblTotalAndTax.Content = TotalPrice() + TotalPrice() * TAX_RATE;
            lblTotalDiscount.Content = totalDiscount;
            FillYear();
            FillMonth();


           }
        private void FillYear() {
            int  dt = DateTime.Now.Year;
            for (int i = 0; i <= 20; i++) {
                cmbYear.Items.Add(dt + i);
            }
            cmbYear.SelectedIndex = 0;

        }
        private void FillMonth()
        {
           
            for (int i = 1; i <= 12; i++)
            {
                cmbMonth.Items.Add(String.Format("{0:00}", i));
            }
           cmbMonth.SelectedIndex = 0;

        }
        private decimal TotalPrice()
        {
            decimal totalPrice = 0;
            foreach (OrderItem or in orderItems)
            {
                totalPrice += or.SalePricePerUnit * or.NumberOfUnit;
            }
            return totalPrice;
        }

        private void cmbPaymenthMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPaymenthMethod.SelectedItem == null) { btnSubmitOrder.IsEnabled = false; return; }
            if (cmbPaymenthMethod.SelectedValue.ToString() == PaymentMethod.CreditCard.ToString()) {
                cmbCardType.IsEnabled = true;
                cmbYear.IsEnabled = true;
                cmbMonth.IsEnabled = true;
                lblCardNumber.IsEnabled = true;
                lblCardType.IsEnabled = true;
                lblExpirationDate.IsEnabled = true;
                lblSecurityCode.IsEnabled = true;
                lblYYMM.IsEnabled = true;
                txtCardNumber.IsEnabled = true;
                txtSecurityCode.IsEnabled = true;
                lblCheckNumber.IsEnabled = false;
                txtCheckNumber.IsEnabled = false;
               

            }
            else if (cmbPaymenthMethod.SelectedValue.ToString() == PaymentMethod.Cheque.ToString())
            {
                cmbCardType.IsEnabled = false;
                cmbYear.IsEnabled = false;
                cmbMonth.IsEnabled = false;
                lblCardNumber.IsEnabled = false;
                lblCardType.IsEnabled = false;
                lblExpirationDate.IsEnabled = false;
                lblSecurityCode.IsEnabled = false;
                lblYYMM.IsEnabled = false;
                txtCardNumber.IsEnabled = false;
                txtSecurityCode.IsEnabled = false;
                lblCheckNumber.IsEnabled = true;
                txtCheckNumber.IsEnabled = true;


            }
            else
            {
                cmbCardType.IsEnabled = false;
                cmbYear.IsEnabled = false;
                cmbMonth.IsEnabled = false;
                lblCardNumber.IsEnabled = false;
                lblCardType.IsEnabled = false;
                lblExpirationDate.IsEnabled = false;
                lblSecurityCode.IsEnabled = false;
                lblYYMM.IsEnabled = false;
                txtCardNumber.IsEnabled = false;
                txtSecurityCode.IsEnabled = false;
                lblCheckNumber.IsEnabled = false;
                txtCheckNumber.IsEnabled = false;

            }
            btnSubmitOrder.IsEnabled = true;

        }

        private void btnSubmitOrder_Click(object sender, RoutedEventArgs e)
        {


        }
    }
}
