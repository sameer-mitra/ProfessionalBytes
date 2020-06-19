using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace ProBytes.Core
{
    public partial class PBEventWriter
    {
        private int iMaxNumberLines;
        private int iWriteCounter;
        private int iLatency = 1;
        private string sFileName;
        private StreamWriter swWriter;
        private string[] sCurrentEvents;
        private const int DEFAULT_MAX_LINES = 200;
        private const int DEFAULT_LATENCY = 1;

        public PBEventWriter(string fName, int maxLines, int lat)
        {
            sFileName = fName;
            if (maxLines < 1)
            {
                iMaxNumberLines = DEFAULT_MAX_LINES;
            }
            else
            {
                iMaxNumberLines = maxLines;
            }

            if (lat < 1)
            {
                iLatency = DEFAULT_LATENCY;
            }
            else
            {
                iLatency = lat;
            }

            CreateEventLog();
        }

        public PBEventWriter(string fileName, int maxLines)
        {
            sFileName = fileName;
            iLatency = DEFAULT_LATENCY; // Default
            if (maxLines < 1)
            {
                iMaxNumberLines = DEFAULT_MAX_LINES; // Default
            }
            else
            {
                iMaxNumberLines = maxLines;
            }

            CreateEventLog();
        }

        public PBEventWriter(string fileName)
        {
            sFileName = fileName;
            iMaxNumberLines = DEFAULT_MAX_LINES; // Default
            iLatency = DEFAULT_LATENCY; // Default
            CreateEventLog();
        }

        private void CreateEventLog()
        {
            try
            {
                int i;
                i = sFileName.LastIndexOf(@"\");
                string dir = sFileName.Substring(0, i);
                int nRetry = 0;
                int nMaxRetries = 10;
                bool bOK = false;
                while (nRetry < nMaxRetries)
                {
                    try
                    {
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        if (!File.Exists(sFileName))
                        {
                            // Create the log file if it does not exist.
                            swWriter = File.CreateText(sFileName);
                            swWriter.Close();
                            Array.Resize(ref sCurrentEvents, 1);
                        }
                        else
                        {
                            // Return the current Lines.
                            sCurrentEvents = File.ReadAllLines(sFileName);
                        }

                        bOK = true;
                        break;
                    }
                    catch
                    {
                        nRetry = nRetry + 1;
                        Thread.Sleep(100);
                    }
                }

                if (bOK == false)
                    return;
            }
            catch (Exception ex)
            {
                LogError("Create Event Log", ex.ToString());
            }
        }

        public void WritePBEvent(PBEvent hEvent, bool bOverWrite = false)
        {
            try
            {

                // SM 20101001 F03324 Add the process ID to the event log entry.
                hEvent = AddProcessID(hEvent);
                string newLine = hEvent.ToString();
                iWriteCounter = iWriteCounter + 1;
                bool bContinue = true;

                // Prepend the line to the start of the line array.
                // If overwriting, then replace the last event, no need to resize the array. 
                int i;
                if (bOverWrite)
                {
                    PBEvent hTemp;
                    hTemp = PBEvent.ParsePBEvent(sCurrentEvents[0]);
                    if (hTemp.LogActionVerb.ToUpper() == hEvent.LogActionVerb.ToUpper())
                    {
                        sCurrentEvents[0] = newLine;
                        bContinue = false;
                    }
                }

                if (bContinue)
                {
                    if (sCurrentEvents.Length - 1 >= iMaxNumberLines - 1)
                    {
                        // Shift all array elements by 1.
                        for (i = sCurrentEvents.Length - 1; i >= 1; i -= 1)
                            sCurrentEvents[i] = sCurrentEvents[i - 1];
                        sCurrentEvents[0] = newLine;
                    }
                    else
                    {
                        // Redim array AND Shift all array elements by 1.
                        Array.Resize(ref sCurrentEvents, sCurrentEvents.Length + 1 + 1);
                        for (i = sCurrentEvents.Length - 1; i >= 1; i -= 1)
                            sCurrentEvents[i] = sCurrentEvents[i - 1];
                        sCurrentEvents[0] = newLine;
                    }
                }

                // Deal with latency..
                // Only write after a certain number of events.
                if (iWriteCounter % iLatency == 0)
                {
                    int nRetry = 0;
                    int nMaxRetries = 10;
                    bool bOK = false;
                    while (nRetry < nMaxRetries)
                    {
                        try
                        {
                            swWriter = new StreamWriter(sFileName, false);
                            using (swWriter)
                            {
                                var loopTo = sCurrentEvents.Length-1;
                                for (i = 0; i <= loopTo; i += 1)
                                    swWriter.WriteLine(sCurrentEvents[i]);
                            }

                            // Reset iWriteCounter
                            iWriteCounter = 0;
                            bOK = true;
                            break;
                        }
                        catch
                        {
                            nRetry = nRetry + 1;
                            Thread.Sleep(100);
                        }
                    }

                    if (bOK == false)
                        return;
                }
            }
            catch (Exception ex)
            {
                LogError("Write HDM Event", ex.ToString());
            }
        }

        public void Write()
        {
            try
            {
                int nRetry = 0;
                int nMaxRetries = 10;
                bool bOK = false;
                while (nRetry < nMaxRetries)
                {
                    try
                    {
                        swWriter = new StreamWriter(sFileName, false);
                        int i;
                        using (swWriter)
                        {
                            var loopTo = sCurrentEvents.Length-1;
                            for (i = 0; i <= loopTo; i += 1)
                                swWriter.WriteLine(sCurrentEvents[i]);
                        }

                        // Reset iWriteCounter
                        iWriteCounter = 0;
                        bOK = true;
                        break;
                    }
                    catch
                    {
                        nRetry = nRetry + 1;
                        Thread.Sleep(100);
                    }
                }

                if (bOK == false)
                    return;
            }
            catch (Exception ex)
            {
                LogError("Close/Write", ex.ToString());
            }
        }

        public void Close()
        {
            Write();
        }

        private void LogError(string sSource, string sMessage)
        {
            try
            {
                StreamWriter SW;
                string errorFile = Assembly.GetExecutingAssembly().Location + @"\PLHDMWRITER.TXT";
                if (!File.Exists(errorFile))
                {
                    SW = File.CreateText(errorFile);
                }
                else
                {
                    SW = new StreamWriter(errorFile, true);
                }

                string sLine = "";
                sLine = sLine + DateTime.Now.ToString( "yyyyMMdd hh:mm:ss tt");
                sLine = sLine + ": ";
                sLine = sLine + sSource.PadRight(20, ' ');
                sLine = sLine + ": ";
                sLine = sLine + sMessage.Replace("\r\n", " ");
                using (SW)
                    SW.WriteLine(sLine);
            }
            catch (Exception)
            {
            }
        }

        public void WritePBEvent(DateTime logTime, string logCompany, string logActionVerb, string logActionTarget, string logDetails, int logStageProgress, int logTotalProgress)
        {
            PBEvent hEvent;
            hEvent = new PBEvent(logTime, logCompany, logActionVerb, logActionTarget, logDetails, logStageProgress, logTotalProgress);
            WritePBEvent(hEvent);
        }

        public void WritePBEvent(string logCompany, string logActionVerb, string logActionTarget, string logDetails, int logStageProgress, int logTotalProgress)
        {
            PBEvent hEvent;
            hEvent = new PBEvent(logCompany, logActionVerb, logActionTarget, logDetails, logStageProgress, logTotalProgress);
            WritePBEvent(hEvent);
        }

        public void WritePBEvent(DateTime logTime, string logActionVerb, string logActionTarget, string logDetails)
        {
            PBEvent hEvent;
            hEvent = new PBEvent(logTime, logActionVerb, logActionTarget, logDetails);
            WritePBEvent(hEvent);
        }

        public void WritePBEvent(string logCompany, string logActionVerb, string logActionTarget, string logDetails)
        {
            PBEvent hEvent;
            hEvent = new PBEvent(logCompany, logActionVerb, logActionTarget, logDetails);
            WritePBEvent(hEvent);
        }

        public string LogFileName // BP 20100419 F02330 Maintain Log History
        {
            get
            {
                return sFileName;
            }
        }

        // SM 20101001 F03324 Add the process ID to the event log entry.
        private static PBEvent AddProcessID(PBEvent hEvent)
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                string sProcessID = currentProcess.Id.ToString();
                sProcessID = "[" + sProcessID.PadLeft(6) + "] ";

                // Add the process ID to the log details.
                hEvent.LogDetails = sProcessID + hEvent.LogDetails;
            }
            catch (Exception)
            {
            }
            finally
            {
            }

            return hEvent;
        }
    }
}