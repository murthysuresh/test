using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;

using System.ComponentModel;

namespace TWSLearn
{
    //Main class for tick capture
    public class TickMain
    {
        //Private and public accessor for the IBClient object
        private IBClient _client = null;
        public IBClient client
        {
            get { return _client; }
            set
            {
                _client = value;
                //If the client is connected, then set the queue.stop = false
                if (_client.Connected)
                    queue.stop = false;
                else
                    queue.stop = true;
            }

        }

        public List<String> stockList = new List<string>();
        public MySqlConnection conn = null;
        public bool doGet = true;

        private TickQueue queue = new TickQueue();
        private BackgroundWorker bg = new BackgroundWorker();
        private Dictionary<int, String> tickId = new Dictionary<int, string>();
        private int tickIndex = 0;
        private object lockObj = new object();

        //Constructors
        public TickMain()
        {
            initialize();
        }

        public TickMain(IBClient client, List<String> stockList, MySqlConnection conn)
        {
            this.client = client;
            this.stockList = stockList;
            this.conn = conn;
            initialize();
        }

        //Initialization method
        private void initialize()
        {
            //Setup the background worker to run the queue
            bg.DoWork += new DoWorkEventHandler(bg_DoWork);

            //Connect to MySQL if we haven't already
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();
            //Don't process the queue if not connected to IB
            if (!client.Connected)
                queue.stop = true;

            //Set the MySQL connection for hte queue
            queue.conn = conn;

            //Get the next value of the queue index
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "select coalesce(max(idticks),0) from ticks";
                MySqlDataReader Reader;

                Reader = cmd.ExecuteReader();
                Reader.Read();
                tickIndex = Reader.GetInt32(0) + 1;
                Reader.Close();
            }
        }

        //Method for getting market prices
        public void Run()
        {
            if (client.Connected)
            {
                //Set up the event handlers for the ticks
                client.TickPrice += new EventHandler<TickPriceEventArgs>(client_TickPrice);
                client.TickSize += new EventHandler<TickSizeEventArgs>(client_TickSize);
                
                //Initialize a counter for stock symbols
                int i = 1;
                
                //Start the queue
                bg.RunWorkerAsync();

                //Request market data for each stock in the stockList
                foreach (String str in stockList)
                {
                    tickId.Add(i, str);
                    client.RequestMarketData(i, new Equity(str), null, false, false);
                    i++;
                }

                //Hang out until told otherwise
                while (doGet)
                {
                    System.Threading.Thread.Sleep(100);
                }

                //Remove event handlers
                Console.WriteLine("Shutting Down TickMain");
                client.TickPrice -= new EventHandler<TickPriceEventArgs>(client_TickPrice);
                client.TickSize -= new EventHandler<TickSizeEventArgs>(client_TickSize);
                queue.stop = true;
            }
        }

        //Event handler for TickSize events
        void client_TickSize(object sender, TickSizeEventArgs e)
        {
            //Get the symbol from the dictionary
            String symbol = tickId[e.TickerId];
            int i = 0;
            
            //As this is asyncronous, lock and get the current tick index
            lock (lockObj)
            {
                i = tickIndex;
                tickIndex++;
            }

            //Create a tick object and enqueue it
            Tick tick = new Tick(EnumDescConverter.GetEnumDescription(e.TickType), 
                e.Size, symbol, i);
            queue.Add(tick);
        }

        //Event Handler for TickPrice events
        void client_TickPrice(object sender, TickPriceEventArgs e)
        {
            //Get the symbol from the dictionary
            String symbol = tickId[e.TickerId];
            int i = 0;

            //As this is asyncronous, lock and get the current tick index
            lock (lockObj)
            {
                i = tickIndex;
                tickIndex++;
            }

            //Create a tick object and enqueue it
            Tick tick = new Tick(EnumDescConverter.GetEnumDescription(e.TickType), 
                e.Price, symbol, i);
            queue.Add(tick);
        }

        //BackgroundWorker delegate to run the queue.
        private void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            queue.Run();
        }

    }
}
