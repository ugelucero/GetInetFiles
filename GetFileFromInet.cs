using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace UgeLucero.Utilities
{
    class GetFileFromInet
    {
        #region Datos Estaticos.
        static int exit = -1;
        static int paso = 0;
        static TimeSpan miliToTimeOut;
        static TimeSpan miliToWarning;
        static TimeSpan miliToStoped;
        static int numberToStoped;
        static DateTime startTime = System.DateTime.Now;
        static DateTime previousPrecentTime = System.DateTime.Now;
        static int previousPercent = 0;
        static int stopedCount=0;
        #endregion
        
        static int Main(string[] args)
        {
            //TODO: controlar los parametros de entrada y imprimir ayuda.
            using (WebClient wc = new WebClient())
            {
                if (startControl(args))
                {
                    #region Configuracion

                    if (args.Length >= 3)
                    {
                        miliToTimeOut = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(args[2]));
                        miliToWarning = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(args[2]) / 50);
                        miliToStoped = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(args[2]) / 25);
                        numberToStoped = 2;
                    }
                    else
                    {
                        miliToTimeOut = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(ConfigurationManager.AppSettings["TotalTimeOut"]));
                        miliToWarning = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(ConfigurationManager.AppSettings["WarnigTimeOut"]));
                        miliToStoped = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(ConfigurationManager.AppSettings["StopedTimeOut"]));
                        numberToStoped = Convert.ToInt32(ConfigurationManager.AppSettings["StopedNumOcurs"]);
                    }
                    #endregion

                    //Todo lo comenmtado seria para hacer una descarga
                    wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                    Console.WriteLine("Pulse intro para terminar la descarga.");
                    wc.DownloadFileAsync(new Uri(args[0]), args[1]);
                    ConsoleKeyInfo exitKeyCode = new ConsoleKeyInfo();
                    //int exitKeyCode = -1;
                    while (exit == -1)
                    {
                        System.Threading.Thread.Sleep(100);
                        if (System.DateTime.Now - startTime >= miliToTimeOut)
                        {
                            Console.WriteLine();
                            Console.WriteLine("TimeOut: superate max TimeOut");
                            exit = -100;
                        }
                        else
                        {
                            double incrementTime = (DateTime.Now - previousPrecentTime).TotalMilliseconds;
                            if (incrementTime > miliToStoped.TotalMilliseconds)
                            {
                                stopedCount++;
                            }
                            if (stopedCount >= numberToStoped)
                            {
                                Console.WriteLine();
                                Console.WriteLine("TimeOut by slow dowload speed.");
                                exit = -200;
                            }

                        }

                        while (Console.KeyAvailable)
                        {
                            exitKeyCode = Console.ReadKey(true);
                            // if (exitKeyCode == 10 || exitKeyCode == 13)
                            if (exitKeyCode.Key == ConsoleKey.Enter)
                                exit = 1;
                        }
                    }
                }
                return exit;
            }
            
        }
        
        static void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            int incrementPercent = e.ProgressPercentage - previousPercent;
            double incrementTime = (DateTime.Now - previousPrecentTime).TotalMilliseconds/(incrementPercent>0?incrementPercent:1);
            if (incrementTime > miliToStoped.TotalMilliseconds)
            {
                stopedCount++;
            }
            else
            {
                stopedCount = 0;
            }
            if (stopedCount >= numberToStoped)
            {
                Console.WriteLine();
                Console.WriteLine("TimeOut by slow dowload speed.");
                exit = -200;
            }
            if (incrementTime >miliToWarning.TotalMilliseconds)
            {
                Console.WriteLine("Warning: slow downloading.");
            }
            int intermediario = paso % 4;
            char animatedUno=' ';
            char animatedDos=' '; 
            switch (intermediario)
            {
                case 0:
                    animatedUno = animatedDos = '-';
                    break;
                case 1:
                    animatedUno = '\\';
                    animatedDos = '/';
                    break;
                case 2:
                    animatedUno = '|';
                    animatedDos = '|';
                    break;
                case 3:
                    animatedUno = '/';
                    animatedDos = '\\';
                    break;
            }
            Console.Write(String.Format("\r{0} {1} {2:n0} % downloaded..",animatedUno,animatedDos, e.ProgressPercentage));
            paso = intermediario==3 ? 0 : paso+1;
            previousPercent = e.ProgressPercentage;
            previousPrecentTime =DateTime.Now;
        }

        static void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Console.WriteLine();
            if (e.Error == null && !e.Cancelled)
            {
                Console.WriteLine("Finalize download.");
                exit = 0;
            }
            else if (e.Cancelled)
            {
                Console.WriteLine("Descarga Cancelada.");
                exit = 1;
            }
            else
            {
                Console.WriteLine("Error en la descarga:" + e.Error.ToString());
                exit = 10;
            }
        }

        static bool startControl(string[] param)
        {
            bool imprimir_ayuda=false;
            if (param.Length < 2 || param.Length > 3)
            {
                Console.WriteLine("Error: incorrect number of params.");
                Console.WriteLine();
                imprimir_ayuda = true;
            } else if(true)
            {
                try { 
                    Uri input = new Uri(param[0]); 
                }
                catch (UriFormatException uex) { 
                    imprimir_ayuda = true;
                    Console.WriteLine("Error: the firts parameter there is not uri.");
                    Console.WriteLine();
                }
                try
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(param[1]);
                    System.IO.DirectoryInfo di = fi.Directory;
                    if (!di.Exists)
                        throw new System.IO.DirectoryNotFoundException("The second parameter isn´t a existen path.");                   
                }
                catch (Exception ex)
                {
                    imprimir_ayuda = true;
                    Console.WriteLine("Error: the second parameter there is not correct file path.");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine();
                }
                if (param.Length==3)
                {
                    try
                    {
                        int input2 = Int32.Parse(param[2]);
                    }
                    catch(Exception ex)
                    {
                        imprimir_ayuda = true;
                        Console.WriteLine("Error: the third parameter there is not valid number of milisecond.");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine();
                    }
                }
            }

            if (imprimir_ayuda)
            {
                Console.WriteLine();
                Console.WriteLine("GetFileFromInet: command line tool for download internet files.");
                Console.WriteLine();
                Console.WriteLine("Ussage: GetFileFromInet  fileToDownload  localPath  [timeOutMiliseconds]");
                Console.WriteLine();
                Console.WriteLine("\tfileToDownload: uri to a internet file. ");
                Console.WriteLine("\tlocalPath: local path for downloaded file. ");
                Console.WriteLine("\ttimeOutMiliseconds: optional. Max milisecond for this download task. ");
            }

            return !imprimir_ayuda;
        }
    }
}
