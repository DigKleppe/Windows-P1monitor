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
        public float voltage;
    }
    public class Log
    {
        public enum LogType { HOURLOG, DAYLOG };
        private const int LOGINTERVAL = 60 * 5;
        private const int MAXDAYLOGVALUES = (24 * 60 * 60) / LOGINTERVAL;
        private const int MAXHOURLOGVALUES = 3600;

        private List<log_t> hourLog;
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
            accumulator.voltage += logValue.voltage;

            if (--logPrescaler == 0)
            {
                logPrescaler = LOGINTERVAL;

                logValue.power = accumulator.power / LOGINTERVAL; // average
                accumulator.power = 0;

                logValue.deliveredPower = accumulator.deliveredPower / LOGINTERVAL; // average
                accumulator.deliveredPower = 0;

                logValue.voltage = accumulator.voltage / LOGINTERVAL;
                accumulator.voltage = 0;

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
