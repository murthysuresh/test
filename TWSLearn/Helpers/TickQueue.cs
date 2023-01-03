using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace TWSLearn
{
    /// <summary>
    /// Class for enqueueing the database writes
    /// </summary>
    public class TickQueue
    {
        // Internal queue of db inserts
        private Queue<Tick> queue = new Queue<Tick>();
        
        // Locking Object.  Necessary as multiple threads will be 
        // accessing this object simultanously
        private Object qLockObj = new object();

        //Connection object to MySQL
        public MySqlConnection conn = null;
        //Flag to stop processing
        public bool stop = false;

        //Method to enqueue a write
        public void Add(Tick item)
        {
            //Lock for single access only
            lock(qLockObj)
            {
                queue.Enqueue(item);
            }
        }

        //Method to write items as they are enqueued
        public void Run()
        {
            int n = 0;
            //Loop while the stop flag is false
            while (!stop)
            {
                //Lock and get a count of the object in the queue
                lock (qLockObj)
                {
                    n = queue.Count;
                }

                //If there are objects in the queue, then process them
                if (n > 0)
                {
                    process();
                }

                //Sleep for .1 seconds before looping again
                System.Threading.Thread.Sleep(100);
            }

            //When the shutdown flag is received, write any 
            //values still in the queue and then stop
            Console.WriteLine("Shutting Down TickQueue; " + queue.Count + " items left");
            process();
        }

        //Method to process items in the queue
        private void process()
        {
            List<Tick> inserts = new List<Tick>();
            int i = 0;
            //Loop through the items in the queue and put them in a list
            lock (qLockObj)
            {
                for (i = 0; i < queue.Count; i++)
                    inserts.Add(queue.Dequeue());
            }

            Console.WriteLine("Processing " + i + " items into database");

            //call insert for each item.
            foreach (Tick t in inserts)
                insert(t);
        }

        //Method to insert a tick
        private void insert(Tick t)
        {
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = t.toInsert();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception exp)
                {
                    Console.WriteLine("OOPS " + exp.Message);
                }
            }
        }
    }
}
