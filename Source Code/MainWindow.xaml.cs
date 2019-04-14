using System;
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
using Excel = Microsoft.Office.Interop.Excel;

namespace atmProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Excel.Application MyApp;
        private Excel.Workbook MyBook;
        private Excel.Worksheet MySheet;
        private Excel.Range MyRange;
        private int rowCount;
        private int currentAcc;
        private int withdrawals;

        public MainWindow()
        {
            InitializeComponent();

            // open and set the excel file full of users
            MyApp = new Excel.Application();
            MyApp.Visible = false;

            string fileName = "accounts.xlsx";
            string path = AppDomain.CurrentDomain.BaseDirectory + fileName;

            MyBook = MyApp.Workbooks.Open(path);
            MySheet = MyBook.Sheets[1];

            // Find range of excel file
            MyRange = MySheet.UsedRange;
            rowCount = MyRange.Rows.Count;

            currentAcc = -1;

            withdrawals = 0;
        }

        // deconstructor - closes and saves excel file
        ~MainWindow()
        {
            MyBook.Save();
            MyBook.Close(true);
            MyApp.Quit();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            double cardNum;
            double cardPin;
            try
            {
                cardNum = Convert.ToDouble(txtCardNum.Text);
                cardPin = Convert.ToDouble(txtCardPin.Password);
            }
            catch (System.FormatException)
            {
                MessageBox.Show("Invalid Login");
                txtCardNum.Text = "";
                txtCardPin.Password = "";
                return;
            }
            bool foundNum = false;
            bool pinMatch = false;

            int x = 0;
            while (x <= rowCount && foundNum == false)
            {
                x++;
                if (cardNum == (double)(MySheet.Cells[x, 1] as Excel.Range).Value)
                {
                    foundNum = true;
                    if (cardPin == (double)(MySheet.Cells[x, 2] as Excel.Range).Value)
                        pinMatch = true;
                }
            }

            // invalid login
            if (!(foundNum && pinMatch))
            {
                MessageBox.Show("Invalid Login");
                txtCardNum.Text = "";
                txtCardPin.Password = "";
                return;
            }

            // set current account to the current found row
            currentAcc = x;

            // clear the text boxes
            txtCardNum.Text = "";
            txtCardPin.Password = "";

            // valid login
            LoadMenu(currentAcc);
        }

        private void btnBalance_Click(object sender, RoutedEventArgs e)
        {
            // hide the other possible menus
            HideTransHistory();
            HideWithdraw();

            // display the balance
            lblBalanceAmt.Content = String.Format("{0:C}", (double)(MySheet.Cells[currentAcc, 5] as Excel.Range).Value);

            // Make the labels visible
            lblBalance.Visibility = Visibility.Visible;
            lblBalanceAmt.Visibility = Visibility.Visible;
        }

        private void btnWithdraw_Click(object sender, RoutedEventArgs e)
        {
            // hide the other possible menus
            HideTransHistory();
            HideBalance();

            // display the withdrawal menu
            btnWith.Visibility = Visibility.Visible;
            txtWith.Visibility = Visibility.Visible;
            lblWith.Visibility = Visibility.Visible;
            lblWithAmt.Visibility = Visibility.Visible;
            lblWithdrawSuccess.Visibility = Visibility.Visible;
        }


        private void btnWith_Click(object sender, RoutedEventArgs e)
        {
            double amount = 0;
            
            lblWithdrawSuccess.Content = "";
            try
            {
                amount = Convert.ToDouble(txtWith.Text);
            }
            catch (System.FormatException)
            {
                MessageBox.Show("Please enter a valid amount.");
                txtWith.Text = "";
                return;
            }

            if (withdrawals >= 5)
            {
                MessageBox.Show("Maximum of 5 withdrawals per login.");
                txtWith.Text = "";
                return;
            }

            if (amount > 1000)
            {
                MessageBox.Show("Maximum of $1000.00 per withdrawal.");
                txtWith.Text = "";
                return;
            }

            if (amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount.");
                txtWith.Text = "";
                return;
            }

            double bal = (double)(MySheet.Cells[currentAcc, 5] as Excel.Range).Value;
            bal -= amount;
            MySheet.Cells[currentAcc, 5] = bal;

            lblWithdrawSuccess.Content = "Successful withdrawal of " + String.Format("{0:C}", amount) + ".";

            // add the new withdrawal to the excel data.
            // amounts
            amount *= -1;
            MySheet.Cells[currentAcc, 10] = MySheet.Cells[currentAcc, 9];
            MySheet.Cells[currentAcc, 9] = MySheet.Cells[currentAcc, 8];
            MySheet.Cells[currentAcc, 8] = MySheet.Cells[currentAcc, 7];
            MySheet.Cells[currentAcc, 7] = MySheet.Cells[currentAcc, 6];
            MySheet.Cells[currentAcc, 6] = amount;

            // dates
            MySheet.Cells[currentAcc, 15] = MySheet.Cells[currentAcc, 14];
            MySheet.Cells[currentAcc, 14] = MySheet.Cells[currentAcc, 13];
            MySheet.Cells[currentAcc, 13] = MySheet.Cells[currentAcc, 12];
            MySheet.Cells[currentAcc, 12] = MySheet.Cells[currentAcc, 11];
            MySheet.Cells[currentAcc, 11] = DateTime.Now;

            // Increase the number of withdrawals by 1
            withdrawals++;

            // Reset text box
            txtWith.Text = "";
        }

        private void btnTransHistory_Click(object sender, RoutedEventArgs e)
        {
            // hide the other possible menus
            HideBalance();
            HideWithdraw();

            // set the values to the labels
            lblDate1.Content = ((DateTime)(MySheet.Cells[currentAcc, 11] as Excel.Range).Value).ToString("MM/dd/yy");
            lblDate2.Content = ((DateTime)(MySheet.Cells[currentAcc, 12] as Excel.Range).Value).ToString("MM/dd/yy");
            lblDate3.Content = ((DateTime)(MySheet.Cells[currentAcc, 13] as Excel.Range).Value).ToString("MM/dd/yy");
            lblDate4.Content = ((DateTime)(MySheet.Cells[currentAcc, 14] as Excel.Range).Value).ToString("MM/dd/yy");
            lblDate5.Content = ((DateTime)(MySheet.Cells[currentAcc, 15] as Excel.Range).Value).ToString("MM/dd/yy");
            lblTrans1.Content = "$" + String.Format("{0:0.00}", (double)(MySheet.Cells[currentAcc, 6] as Excel.Range).Value);
            lblTrans2.Content = "$" + String.Format("{0:0.00}", (double)(MySheet.Cells[currentAcc, 7] as Excel.Range).Value);
            lblTrans3.Content = "$" + String.Format("{0:0.00}", (double)(MySheet.Cells[currentAcc, 8] as Excel.Range).Value);
            lblTrans4.Content = "$" + String.Format("{0:0.00}", (double)(MySheet.Cells[currentAcc, 9] as Excel.Range).Value);
            lblTrans5.Content = "$" + String.Format("{0:0.00}", (double)(MySheet.Cells[currentAcc, 10] as Excel.Range).Value);

            // make the labels visible
            lblDate1.Visibility = Visibility.Visible;
            lblDate2.Visibility = Visibility.Visible;
            lblDate3.Visibility = Visibility.Visible;
            lblDate4.Visibility = Visibility.Visible;
            lblDate5.Visibility = Visibility.Visible;
            lblTrans1.Visibility = Visibility.Visible;
            lblTrans2.Visibility = Visibility.Visible;
            lblTrans3.Visibility = Visibility.Visible;
            lblTrans4.Visibility = Visibility.Visible;
            lblTrans5.Visibility = Visibility.Visible;
            lblTransHistTitle.Visibility = Visibility.Visible;
        }

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            LoadLogin();
        }

        private void LoadMenu(int account)
        {
            string firstName = (String)(MySheet.Cells[account, 4] as Excel.Range).Value;
            string lastName = (String)(MySheet.Cells[account, 3] as Excel.Range).Value;

            // hide the login screen
            brdLogin.Visibility = Visibility.Hidden;
            txtCardNum.Visibility = Visibility.Hidden;
            txtCardPin.Visibility = Visibility.Hidden;
            lblCardNum.Visibility = Visibility.Hidden;
            lblCardPin.Visibility = Visibility.Hidden;
            btnLogin.Visibility = Visibility.Hidden;

            // show main account menu
            lblUserName.Content = firstName + " " + lastName;
            lblUserName.Visibility = Visibility.Visible;
            brd2.Visibility = Visibility.Visible;
            lstMenu.Visibility = Visibility.Visible;
            btnBalance.Visibility = Visibility.Visible;
            btnWithdraw.Visibility = Visibility.Visible;
            btnTransHistory.Visibility = Visibility.Visible;
            btnLogOut.Visibility = Visibility.Visible;
        }

        private void LoadLogin()
        {
            // reset current account
            currentAcc = -1;
            withdrawals = 0;

            // Hide the main account screen
            lblUserName.Visibility = Visibility.Hidden;
            brd2.Visibility = Visibility.Hidden;
            lstMenu.Visibility = Visibility.Hidden;
            btnBalance.Visibility = Visibility.Hidden;
            btnWithdraw.Visibility = Visibility.Hidden;
            btnTransHistory.Visibility = Visibility.Hidden;
            btnLogOut.Visibility = Visibility.Hidden;
            HideBalance();
            HideWithdraw();
            HideTransHistory();
            lblUserName.Content = "";

            // show login screen
            brdLogin.Visibility = Visibility.Visible;
            txtCardNum.Visibility = Visibility.Visible;
            txtCardPin.Visibility = Visibility.Visible;
            lblCardNum.Visibility = Visibility.Visible;
            lblCardPin.Visibility = Visibility.Visible;
            btnLogin.Visibility = Visibility.Visible;
        }

        private void HideBalance()
        {
            // hide balance menu
            lblBalance.Visibility = Visibility.Hidden;
            lblBalanceAmt.Visibility = Visibility.Hidden;
        }
        
        private void HideWithdraw()
        {
            // hide withdraw menu
            txtWith.Visibility = Visibility.Hidden;
            btnWith.Visibility = Visibility.Hidden;
            lblWith.Visibility = Visibility.Hidden;
            lblWithAmt.Visibility = Visibility.Hidden;
            lblWithdrawSuccess.Visibility = Visibility.Hidden;
            lblWithdrawSuccess.Content = "";
        }

        private void HideTransHistory()
        {
            // hide transaction history menu
            // clear the labels
            lblDate1.Content = "";
            lblDate2.Content = "";
            lblDate3.Content = "";
            lblDate4.Content = "";
            lblDate5.Content = "";
            lblTrans1.Content = "";
            lblTrans2.Content = "";
            lblTrans3.Content = "";
            lblTrans4.Content = "";
            lblTrans5.Content = "";

            // hide the menu
            lblTransHistTitle.Visibility = Visibility.Hidden;
            lblDate1.Visibility = Visibility.Hidden;
            lblDate2.Visibility = Visibility.Hidden;
            lblDate3.Visibility = Visibility.Hidden;
            lblDate4.Visibility = Visibility.Hidden;
            lblDate5.Visibility = Visibility.Hidden;
            lblTrans1.Visibility = Visibility.Hidden;
            lblTrans2.Visibility = Visibility.Hidden;
            lblTrans3.Visibility = Visibility.Hidden;
            lblTrans4.Visibility = Visibility.Hidden;
            lblTrans5.Visibility = Visibility.Hidden;
        }
    }
}