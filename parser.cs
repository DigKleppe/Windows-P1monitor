using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

/*
0-0:1.0.0(231021110048S) // timestamp
0-0:96.1.1(4530303533303038333837313934353231) // equipment ident
1-0:1.8.1(000525.570*kWh) // deliverd energy  to client Tariff 1
1-0:1.8.2(000530.217*kWh) // deliverd energy to client Tariff 2
1-0:2.8.1(000001.552*kWh) // deliverd energy by client Tariff 2
1-0:2.8.2(000000.000*kWh) // deliverd energy by client Tariff 2
0-0:96.14.0(0001)	  // tariff indicator
1-0:1.7.0(00.079*kW)      // actual power deleverd
1-0:2.7.0(00.000*kW)	// actual received
0-0:96.7.21(00012)	// nr power failures
0-0:96.7.9(00004)	// nr long power failures
1-0:99.97.0(2)(0-0:96.7.19)(210827121443S)(0000010761*s)(210827131420S)(0000000326*s) // Power Failure Event Log (long power failures)
1-0:32.32.0(00008)	// Number of voltage sags in phase L1
1-0:32.36.0(00001)	// Number of voltage swells in phase L1
0-0:96.13.0()		// ext message max 1024 characters.
1-0:32.7.0(227.5*V)	// Instantaneous voltage L1 in V resolution
1-0:31.7.0(000*A)	// Instantaneous current L1 in A resolution.
1-0:21.7.0(00.078*kW)	// instantaneous active power L1 (+P) in W resolution
1-0:22.7.0(00.000*kW)	// Instantaneous active power L1 (-P) in W resolution
*/


namespace P1monitor
{
    internal class p1Var
    {
        public string p1ID; // id from P1 specification
        public string name; // name
        public int function; // special function
        public p1Var(string p1ID, string name, int function)
        {
            this.p1ID = p1ID;
            this.name = name;
            this.function = function;
        }
    };

    internal class p1Vars : List<p1Var>
    {
        public void Add(string p1ID, string name, int function)
        {
            Add(new p1Var(p1ID, name, function));
        }
    }

    public class Parser
    {
        public Log log = new Log();

        p1Var[] p1Vars = {
            new p1Var( "1-0:21.7.0", "Actueel opgenomen vermogen L1",1),
            new p1Var(  "1-0:41.7.0", "Actueel opgenomen vermogen L2",1),
            new p1Var(  "1-0:61.7.0", "Actueel opgenomen vermogen L3",1),
            new p1Var(  "1-0:22.7.0", "Actueel geleverd vermogen L1",2),
            new p1Var(  "1-0:42.7.0", "Actueel geleverd vermogen L2",2),
            new p1Var(  "1-0:62.7.0", "Actueel geleverd vermogen L3",2),
            new p1Var(  "1-0:32.7.0", "Spanning L1",3),
            new p1Var(  "1-0:52.7.0", "Spanning L2",3),
            new p1Var(  "1-0:72.7.0", "Spanning L3",3),
            new p1Var(  "1-0:1.8.1",  "Opgenomen energie tarief 1",0 ),
            new p1Var(  "1-0:1.8.2",  "Opgenomen energie tarief 2",0 ),
            new p1Var(  "1-0:2.8.1",  "Geleverde energie tarief 1",0 ),
            new p1Var(  "1-0:2.8.2",  "Geleverde energie tarief 2",0 ),
            new p1Var(  "0-0:96.7.21","Korte onderbrekingen",0),
            new p1Var(  "0-0:96.7.9", "Lange onderbrekingen",0 ),
            new p1Var(  "1-0:99.97.0","Onderbrekingslog",4 ),
            new p1Var(  "", "",0 ) 
        };

        //    p1Vars p1VarTable = new p1Vars {
        //{ "1-0:21.7.0", "Actueel opgenomen vermogen L1",1},
        //{ "1-0:41.7.0", "Actueel opgenomen vermogen L2",1},
        //{ "1-0:61.7.0", "Actueel opgenomen vermogen L3",1},
        //{ "1-0:22.7.0", "Actueel geleverd vermogen L1",2},
        //{ "1-0:42.7.0", "Actueel geleverd vermogen L2",2},
        //{ "1-0:62.7.0", "Actueel geleverd vermogen L3",2},
        //{ "1-0:32.7.0", "Spanning L1",3},
        //{ "1-0:52.7.0", "Spanning L2",3},
        //{ "1-0:72.7.0", "Spanning L3",3},
        //{ "1-0:1.8.1",  "Opgenomen energie tarief 1",0 },
        //{ "1-0:1.8.2",  "Opgenomen energie tarief 2",0 },
        //{ "1-0:2.8.1",  "Geleverde energie tarief 1",0 },
        //{ "1-0:2.8.2",  "Geleverde energie tarief 2",0 },
        //{ "0-0:96.7.21","Korte onderbrekingen",0},
        //{ "0-0:96.7.9", "Lange onderbrekingen",0 },
        //{ "1-0:99.97.0","Onderbrekingslog",4 },
        //{ "", "",0 },
        //};


        // splits (000001.552*kWh)  into strings[] "1.552" and "kwh"

        string[] parseValue ( String valuestr) // (000525.570*kWh)
        {
            string[] val =  valuestr.Split(new char[] { '*' });
           // if ( val.Length ==2 )
            {
                val[0] = val[0].TrimStart('0'); // skip leading zeros
                if (val[0].StartsWith("."))
                    val[0] = "0" + val[0]; 
            }       
            return val;  
        }

        //int copyName(char* src, char* dest)
        //{
        //    return sprintf(dest, "%s=", src);
        //}
        //// reads p1Buffer, searches for IDs , if found add corresponding name and value to p1OutData;

        public List <string> parseP1data(string p1Buffer)
        {
            float f;
            log_t logValue = new log_t();
            logValue.power = 0;
            logValue.deliveredPower = 0;
            string[] lines = p1Buffer.Split(new char[] { '\n' });
            List<string> p1OutBuffer;
            p1OutBuffer = new List<string>();
            string outLine;
            string[] valuestr;

            foreach (p1Var pVar in p1Vars)
            {
                foreach (string line in lines)
                {
                    if (line.Contains(pVar.p1ID))
                    {
                        outLine = pVar.name + ";"; 
                        
                        string[] items = line.Split(new char[] { '(' });
                        valuestr = parseValue(items[0]);

                        switch ( pVar.function)
                        {
                            case 0:
                                //  1 - 0:32.32.0(00008)	// Number of voltage sags in phase L1
                                outLine = outLine + valuestr[0];
                                break;

                            case 1: // Power used, set from kW to W 1-0:1.7.0(00.079*kW)      // actual power deleverd to client Tariff 1
                                f = float.Parse(valuestr[0])  * 1000;
                                logValue.power += f; // add power of 3 phases ( if present)
                                outLine = outLine + f.ToString() + " W";
                                
                                break;

                            case 2: // Power delivered, set from kW to W
                                f = float.Parse(valuestr[0]) * 1000;
                                logValue.deliveredPower += f; // add power of 3 phases ( if present)
                                outLine = outLine + f.ToString() + " W";
                                 break;

                            case 3:  // 1-0:32.7.0(227.5*V)	// Instantaneous voltage L1 in V resolution voltage
                                p1OutBuffer.Add(valuestr[0] + " V" );
                                outLine = outLine + valuestr[0] + " V" ; 
                                break;

                            case 4:   // power failures log eg 1-0:99.97.0(2)(0-0:96.7.19)(210827121443S)(0000010761*s)(210827131420S)(0000000326*s)
                                break;

                        }
                        p1OutBuffer.Add(outLine);
                    }
                }
            }
            log.addToLog(logValue);
            return p1OutBuffer;
        }
    }
}
