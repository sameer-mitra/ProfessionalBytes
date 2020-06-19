using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

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
        public PBEvent(string logCompany, string logActionVerb, string logActionTarget, string logDetails, int logStageProgress, int logTotalProgress)
        {
            this.LogTimeSpecified = false;
            this.LogCompany = logCompany;
            this.LogActionVerb = logActionVerb;
            this.LogActionTarget = logActionTarget;
            this.LogDetails = logDetails.Replace("\r\n", "");
            this.LogStageProgress = logStageProgress;
            this.LogTotalProgress = logTotalProgress;
        }

        public PBEvent(DateTime logTime, string logActionVerb, string logActionTarget, string logDetails)
        {
            this.LogTime = logTime;
            this.LogTimeSpecified = true;
            this.LogCompany = LogCompany;
            this.LogActionVerb = logActionVerb;
            this.LogActionTarget = logActionTarget;
            this.LogDetails = logDetails.Replace("\r\n", "");
            this.LogStageProgress = -1;
            this.LogTotalProgress = 999;
        }

        public PBEvent(string logCompany, string logActionVerb, string logActionTarget, string logDetails)
        {
            this.LogTimeSpecified = false;
            this.LogCompany = logCompany;
            this.LogActionVerb = logActionVerb;
            this.LogActionTarget = logActionTarget;
            this.LogDetails = logDetails.Replace("\r\n", "");
            this.LogStageProgress = -1;
            this.LogTotalProgress = 999;
        }

        public int CompareTo(object obj)
        {
            PBEvent otherEvent = (PBEvent)obj;

            if (otherEvent == null)
            {
                throw new ArgumentException("Object is not PBEvent.");
            }
            else
            {
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

        public static PBEvent ParsePBEvent(string sEvent)
        {
            int i;
            int j;
            var dTime = default(DateTime);
            string sTemp;
            string sCompany;
            string sVerb;
            string sTarget;
            string sDetails;
            int iStageProgress;
            int iTotalProgress;
            bool bTimeParsed = false;

            // BM 20140908 F08187 - Handle non english am/pm while using ParseExact method
            var provider = CultureInfo.InvariantCulture;
            if (CultureInfo.CurrentCulture.TextInfo.ANSICodePage != 1252)
            {
                provider = CultureInfo.CurrentCulture;
            }


            // SM 20100212 F01665 Modified this function to return a default HDM Event when the given string is unable to be parsed.
            try
            {
                if (string.IsNullOrEmpty(sEvent))
                {
                    return CreateDefaultPBEvent(DateTime.Now);
                }

                // Parse the given string to extract variables..
                // SM 20100212 F01665 Add try and catch blocks to preserve as much information as possible.. 
                // only set default values for variables which are in error - decided against this approach as..
                // we would still get incorrect variables due to shifting of the log entry - this could be misleading.

                i = nLogTimeLength - 1;
                sTemp = sEvent.Substring(0, i);
                sTemp = sTemp.Trim();
                // BM 20140908 F08187 - Handle non english am/pm while using ParseExact method
                // dTime = DateTime.ParseExact(sTemp, "yyyyMMdd hh:mm:ss.ff tt", System.Globalization.CultureInfo.InvariantCulture)
                try
                {
                    dTime = DateTime.ParseExact(sTemp, "yyyyMMdd hh:mm:ss.ff tt", provider);
                }
                catch (Exception)
                {
                    dTime = DateTime.ParseExact(sTemp, "yyyyMMdd hh:mm:ss.ff tt", CultureInfo.InvariantCulture);
                }

                bTimeParsed = true;
                j = i;
                i = i + nLogCompanyLength;
                sCompany = sEvent.Substring(j, nLogCompanyLength);
                sCompany = sCompany.Trim();
                j = i;
                i = i + nLogActionVerbLength;
                sVerb = sEvent.Substring(j, nLogActionVerbLength);
                sVerb = sVerb.Trim();
                j = i;
                i = i + nLogActionTargetLength;
                sTarget = sEvent.Substring(j, nLogActionTargetLength);
                sTarget = sTarget.Trim();
                j = i;
                i = i + nLogStageProgressLength;
                sTemp = sEvent.Substring(j, nLogStageProgressLength);
                sTemp = sTemp.Trim();
                if (!string.IsNullOrEmpty(sTemp))
                {
                    iStageProgress = int.Parse (sTemp);
                }
                else
                {
                    iStageProgress = -1;
                }

                j = i;
                i = i + nLogTotalProgressLength;
                sTemp = sEvent.Substring(j, nLogTotalProgressLength);
                sTemp = sTemp.Trim();
                if (!string.IsNullOrEmpty(sTemp))
                {
                    iTotalProgress = int.Parse(sTemp);
                }
                else
                {
                    iTotalProgress = 999;
                }

                j = i;
                sDetails = sEvent.Substring(j);
                sDetails = sDetails.Trim();
                sDetails = sDetails.Replace("\r\n", "");
                var hEvent = new PBEvent(dTime, sCompany, sVerb, sTarget, sDetails, iStageProgress, iTotalProgress);
                return hEvent;
            }
            catch
            {
                // SM 20100212 F01665 return a default HDM Event when the given string is unable to be parsed.
                if (bTimeParsed)
                {
                    return CreateDefaultPBEvent(dTime);
                }
                else
                {
                    return CreateDefaultPBEvent(DateTime.Now);
                }
            }
        }

        private static PBEvent CreateDefaultPBEvent(DateTime dTime)
        {
            // SM 20100212 F01665 return a default HDM Event
            string sCompany = "COMPANY NAME";
            string sVerb = "ACTION VERB";
            string sTarget = "ACTION TARGET";
            string sDetails = "This is a default HDM event, created as an event log entry in the file is not in the specified format and is unable to be parsed. Please check the log file.";
            int iStageProgress = -1;
            int iTotalProgress = 999;
            var hEvent = new PBEvent(dTime, sCompany, sVerb, sTarget, sDetails, iStageProgress, iTotalProgress);
            return hEvent;
        }

    }
}
