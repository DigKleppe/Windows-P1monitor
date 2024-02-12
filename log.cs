using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P1monitor
{
    public  class log_t
    {
        public int timeStamp;
        public float power;
        public float deliveredPower;
        public float currentL1;
        public float currentL2;
        public float currentL3;
    }
    public class Log
    {
        public enum LogType { HOURLOG, DAYLOG };
        private const int LOGINTERVAL = 60 * 5;
        private const int MAXDAYLOGVALUES = (24 * 60 * 60) / LOGINTERVAL;
        private const int MAXHOURLOGVALUES = 3600;

        public List<log_t> hourLog;
        private List<log_t> dayLog;
        private int timeStamp = 0;
        private int logPrescaler = LOGINTERVAL;
        
        log_t accumulator;

        public Log()
        {
            hourLog = new List<log_t>();
            dayLog = new List<log_t>();
            accumulator = new log_t();
        }

        public void addToLog(log_t logValue)
        {
            logValue.timeStamp = ++timeStamp;
            accumulator.power += logValue.power;
            accumulator.deliveredPower += logValue.deliveredPower;
            accumulator.currentL1 += logValue.currentL1;
            accumulator.currentL2 += logValue.currentL2;
            accumulator.currentL3 += logValue.currentL3;

            if (--logPrescaler == 0)
            {
                logPrescaler = LOGINTERVAL;

                logValue.power = accumulator.power / LOGINTERVAL; // average
                accumulator.power = 0;

                logValue.deliveredPower = accumulator.deliveredPower / LOGINTERVAL; // average
                accumulator.deliveredPower = 0;

                logValue.currentL1 = accumulator.currentL1 / LOGINTERVAL;
                accumulator.currentL1 = 0;

                logValue.currentL2 = accumulator.currentL2 / LOGINTERVAL;
                accumulator.currentL2 = 0;

                logValue.currentL3 = accumulator.currentL3 / LOGINTERVAL;
                accumulator.currentL3 = 0;

                if (dayLog.Count == MAXDAYLOGVALUES)
                    dayLog.RemoveAt(0);
                dayLog.Add(logValue);
            }
            if (hourLog.Count == MAXHOURLOGVALUES)
                hourLog.RemoveAt(0);
            hourLog.Add(logValue);
        }
    }
}
