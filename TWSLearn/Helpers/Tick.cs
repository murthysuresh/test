using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TWSLearn
{
    public class Tick
    {
        public String type = "";
        public decimal value = 0;
        public String date = System.DateTime.Now.ToString("yyyy-MM-dd");
        public decimal time = (decimal)System.DateTime.Now.ToOADate();
        public String symbol = "";
        public Int32 index = 0;

        public Tick(String type, decimal value, String symbol, Int32 index)
        {
            this.type = type;
            this.value = value;
            this.symbol = symbol;
            this.index = index;
        }

        public Tick() { }

        public String toInsert()
        {
            String output = "insert into ticks (idticks,symbol,date,time,value,type) values (" +
                                index + 
                                ",'" + symbol + 
                                "', DATE('" + date + "')," + 
                                time + "," +
                                value + ",'" + 
                                type + "')";

            return output;
        }
    }
}
