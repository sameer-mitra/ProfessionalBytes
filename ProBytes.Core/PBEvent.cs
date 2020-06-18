using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProBytes.Core
{
    public class PBEvent : IComparable
    {
        const int nLogTimeLength = 24;
        const int nLogCompanyLength = 36;
        const int nLogActionVerbLength = 16;
        const int nLogActionTargetLength = 16;
        const int nLogStageProgressLength = 4;
        const int nLogTotalProgressLength = 4;
        const int nLogDetailsLength = 255;

        public DateTime LogTime { get; set; }
        public bool LogTimeSpecified { get; set; }
        public string LogCompany { get; set; }
        public string LogActionVerb { get; set; }
        public string LogActionTarget { get; set; }
        public string LogDetails { get; set; }
        public int LogStageProgress { get; set; }
        public int LogTotalProgress { get; set; }

        public PBEvent(DateTime logTime, string logCompany, string logActionVerb, string logActionTarget, string logDetails, int logStageProgress, int logTotalProgress)
        {
            this.LogTime = logTime;
            this.LogTimeSpecified = true;
            this.LogCompany = logCompany;
            this.LogActionVerb = logActionVerb;
            this.LogActionTarget = logActionTarget;
            this.LogDetails = logDetails.Replace("\n", "");
            this.LogStageProgress = logStageProgress;
            this.LogTotalProgress = logTotalProgress;
        }


        public int CompareTo(object obj)
        {
            PBEvent otherEvent = (PBEvent) obj;

            if (otherEvent == null){
                throw new ArgumentException("Object is not PBEvent.");
            }
            else{
                if (this.LogTimeSpecified && otherEvent.LogTimeSpecified)
                {
                    if (this.LogTime.CompareTo(otherEvent.LogTime) == 0)
                    {
                        //SM 20100329 F01978 Change the compare logic to return the correct chronological entry.
                        //Cannot differentiate at this point, so always deem this event to be less than the other event.
                        return -1;
                    }
                    else
                    {
                        return this.LogTime.CompareTo(otherEvent.LogTime);
                    }
                }
                else
                {
                    return -1;
                }
            }
        }

        public override string ToString()
        {
            string sTemp;
            StringBuilder SB = new StringBuilder();
            DateTime dNow = DateTime.Now;

            // Date
            if (LogTimeSpecified)
            {
                //SM 20100702 F02875 Fix the formatting so that it is culture independent.
                SB.Append(this.LogTime.ToString("yyyyMMdd hh:mm:ss.ff tt", System.Globalization.CultureInfo.InvariantCulture));
            }
            else
            {
                SB.Append(dNow.ToString("yyyyMMdd hh:mm:ss.ff tt", System.Globalization.CultureInfo.InvariantCulture));
            }
            SB.Append((char)32);

            //Company
            //Will always be less than 35 characters.
            LogCompany = LogCompany.PadRight(nLogCompanyLength, (char)32);
            SB.Append(LogCompany);

            //Action
            if (this.LogActionVerb.Length > nLogActionVerbLength - 1)
            {
                LogActionVerb = LogActionVerb.Substring(0, nLogActionVerbLength - 3) + ".. ";
            }
            else
            {
                LogActionVerb = LogActionVerb.PadRight(nLogActionVerbLength, (char)32);
            }
            SB.Append(LogActionVerb);

            //Target
            if (LogActionTarget.Length > nLogActionTargetLength - 1)
            {
                LogActionTarget = LogActionTarget.Substring(0, nLogActionTargetLength - 3) + ".. ";
            }
            else
            {
                LogActionTarget = LogActionTarget.PadRight(nLogActionTargetLength, (char)32);
            }
            SB.Append(LogActionTarget);

            //Progress
            if (LogStageProgress > -1 && LogStageProgress < 101)
            {
                sTemp = String.Format(LogStageProgress.ToString(), "000");
            }
            else
            {
                sTemp = "   ";
            }
            SB.Append(sTemp);
            SB.Append((char)32);

            if (LogTotalProgress > -1 && LogTotalProgress < 101)
            {
                sTemp = String.Format(LogTotalProgress.ToString(), "000");
            }
            else
            {
                sTemp = "999";
            }
            SB.Append(sTemp);
            SB.Append((char)32);

            //Details
            LogDetails = LogDetails.Replace("\r", "");
            LogDetails = LogDetails.Replace("\n", "");
            
            if (LogDetails.Length > nLogDetailsLength - 1)
            {
                LogDetails = LogDetails.Substring(0, nLogDetailsLength - 3) + "...";
            }
            else
            {
                LogDetails = LogDetails.PadRight(nLogDetailsLength, (char)32);
            }
            SB.Append(LogDetails);

            //New Line
            sTemp = SB.ToString();

            return sTemp;

        }
    }

    
}
