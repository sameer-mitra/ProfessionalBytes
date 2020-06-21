using System;
using System.IO;

// SM 20091201 F01166 Writing to Log.
// A class which reads HDM log files and returns PBEvents.

namespace ProBytes.Core
{
    public partial class PBEventReader
    {
        private string sFileName;
        private StreamReader sReader;

        // A function which reads HDM events from the goven file and returns as strings.
        public string[] ReadPBEventAsStrings(string fName)
        {
            string[] sEvents;
            sEvents = new string[1];
            try
            {
                if (File.Exists(fName))
                {
                    sReader = new StreamReader(fName);
                    int i = 0;
                    string sTemp;
                    while (sReader.Peek() != -1)
                    {
                        Array.Resize(ref sEvents, i + 1);
                        sTemp = sReader.ReadLine();
                        if (!string.IsNullOrEmpty(sTemp))
                        {
                            sEvents[i] = sTemp;
                            i = i + 1;
                        }
                    }

                    sReader.Close();
                    return sEvents;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // A function which reads HDM events from the given file.
        public PBEvent[] ReadPBEvents(string fName)
        {
            PBEvent[] sEvents;
            sEvents = new PBEvent[1];
            try
            {
                if (File.Exists(fName))
                {
                    int nRetry = 0;
                    int nMaxRetries = 2;
                    bool bOK = false;
                    while (nRetry < nMaxRetries)
                    {
                        try
                        {
                            sReader = new StreamReader(fName);
                            int i = 0;
                            string sTemp;
                            while (sReader.Peek() != -1)
                            {
                                sTemp = sReader.ReadLine();
                                if (string.IsNullOrEmpty(sTemp))
                                {
                                }
                                // do nothing
                                else
                                {
                                    Array.Resize( ref sEvents, i + 1);
                                    sEvents[i] = PBEvent.ParsePBEvent(sTemp);
                                    i = i + 1;
                                }
                            }

                            sReader.Close();
                            bOK = true;
                            break;
                        }
                        catch
                        {
                            nRetry = nRetry + 1;
                            System.Threading.Thread.Sleep(100);
                        }
                    }

                    if (bOK == false)
                    {
                        return null;
                    }
                    else
                    {
                        return sEvents;
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // A function which reads HDM events from a file, filtered by the given company.
        public PBEvent[] ReadPBEventsByCompany(string fName, string company)
        {
            PBEvent[] sEvents;
            PBEvent hEvent;
            sEvents = new PBEvent[1];
            try
            {
                if (File.Exists(fName))
                {
                    sReader = new StreamReader(fName);
                    int i = 0;
                    string sTemp;
                    while (sReader.Peek() != -1)
                    {
                        sTemp = sReader.ReadLine();
                        if (!string.IsNullOrEmpty(sTemp))
                        {
                            hEvent = PBEvent.ParsePBEvent(sTemp);
                            if (hEvent.LogCompany.ToUpper() == company.ToUpper())
                            {
                                Array.Resize(ref sEvents, i + 1);
                                sEvents[i] = hEvent;
                                i = i + 1;
                            }
                        }
                    }

                    sReader.Close();
                    return sEvents;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}