using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;
using System.Data.SqlClient;
using System.ComponentModel;

namespace TWSLearn
{
    class Program
    {
        //static MySqlConnection conn = 
        //    new MySqlConnection("server=LOCALHOST;DATABASE=tick_db;USER=dom;PASSWORD=dom123");
        static SqlConnection myConnection = new SqlConnection("user id=sa;" +
                                       "password=qazwsx123;server=localhost;" +
                                       "Trusted_Connection=yes;" +
                                       "database=dbTorontoTrader; " +
                                       "connection timeout=30");

        static TickMain main = null;

        static void Main(string[] args)
        {
            //Open the connections.
            conn.Open();
            IBClient client = new IBClient();
            client.Connect("localhost", 7496, 2);

            //List of stock ticks to get
            List<String> stockList = new List<string>();
            stockList.Add("SPY");
            stockList.Add("SH");
            stockList.Add("LINE");
            stockList.Add("LRE");
            stockList.Add("GLD");
            stockList.Add("DIA");
            stockList.Add("DOG");
            stockList.Add("GLL");
            stockList.Add("DGZ");
            stockList.Add("GDX");
            stockList.Add("NLR");

            
            //Initialize the TickMain object
            main = new TickMain(client, stockList, conn);
            main.doGet = true;

            //Setup a worker to call main.Run() asycronously.
            BackgroundWorker bg = new BackgroundWorker();
            bg.DoWork += new DoWorkEventHandler(bg_DoWork);
            bg.RunWorkerAsync();
            
            //Chill until the user hits enter then stop the TickMain object
            Console.ReadLine();
            main.doGet = false;

            //disconnect
            client.Disconnect();
            
            Console.WriteLine("Hit Enter to Continue");
            Console.ReadLine();
        }

        //Delegate for main.Run()
        static void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            main.Run();
        }


    }
}
