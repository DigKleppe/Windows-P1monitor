using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        public string logFileName = "log.csv";

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


        private string makeLogName(string baseName)
        {
            string str = baseName;
            int idx = baseName.IndexOf(".csv");
            if (idx > 0)
                str = baseName.Substring(0, idx);
            idx = baseName.IndexOf('@');
            if (idx > 0)
                str = str.Substring(0, idx);
            DateTime dateTime = DateTime.Now;
            str = str + '@' + dateTime.ToString("ddMMyyHHmmss") + ".csv";
            return str;
        }

        private string logtoString ( log_t log )
        {
            string str;
            str = log.timeStamp.ToString() +',';
            str += log.power.ToString("0.0") + ',';
            str += log.deliveredPower.ToString("0.0") + ',';
            str += log.currentL1.ToString("0.0") + ',';
            str += log.currentL2.ToString("0.0") + ',';
            str += log.currentL3.ToString("0.0") + "\r\n";
            return str; 
        }

        private string makeHeader( )
        {
            return  "TijdStempel,Verbruikt vermogen,Geleverd vermogen,Stroom L1,Stroom L2, Stroom L3\r\n";
        }

        public void saveLog(LogType type, string name)
        {
            if (type == LogType.DAYLOG)
                name = name + "Hour";
            else
                name = name + "Day";

            string fileName = makeLogName(name);

            if (fileName.Length > 5) // assume valid
            {
                try
                {
                    using (StreamWriter w = File.CreateText(fileName))
                    {
                        w.WriteLine(makeHeader());
                        if (type == LogType.DAYLOG)
                        {
                            foreach (log_t log in dayLog)
                            {
                                w.WriteLine(logtoString(log));
                            }
                        }
                        else
                        {
                            foreach (log_t log in hourLog)
                            {
                                w.WriteLine(logtoString(log));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    DialogResult result;
                    string str = ex.ToString();
                    result = MessageBox.Show("Error saving file: " + fileName + "\r\n" + str, "Error", buttons);
                }
            }
        }

    }
              
}
