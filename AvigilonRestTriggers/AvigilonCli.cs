using AvigilonDotNet;
using AvigilonRestTriggers.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace AvigilonRestTriggers
{
    public class AvigilonCli
    {

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static IAvigilonControlCenter SharedAcc {
            get{
                if (m_sharedAcc == null)
                {
                    InitACC();
                }
                return m_sharedAcc;
            }
        }
        private static AvigilonDotNet.IAvigilonControlCenter m_sharedAcc = null;
        private INvr nvr = null;

        private static void InitACC()
        {
            var currentdir = AssemblyDirectory;
            AvigilonDotNet.AvigilonSdk sdk = new AvigilonDotNet.AvigilonSdk();
            AvigilonDotNet.SdkInitParams initParams = new AvigilonDotNet.SdkInitParams(AvigilonDotNet.AvigilonSdk.MajorVersion, AvigilonDotNet.AvigilonSdk.MinorVersion);
            initParams.SdkPath = @"C:\Program Files\Avigilon\Avigilon Control Center SDK\SDK\AvigilonRedist\win32";

            initParams.AutoDiscoverNvrs = false;
            initParams.ServiceMode = true;

            "InitACC:AvigilonSdk.CreateInstance...".Log();
            AvigilonDotNet.IAvigilonControlCenter acc = sdk.CreateInstance(initParams);
            if (acc == null)
            {
                "AvigilonSdk.CreateInstance:Error creating new instance".LogError();
            }
            else "AvigilonSdk.CreateInstance:OK".Log();

            m_sharedAcc = acc;
        }

        public AvigilonCli()
        {
            //this.NvrAddress = _nvrAddress;

            
            
            //nvr = addNvr("192.168.15.8");
            
        }

        public List<INvr> GetAllNvrs()
        {
            return SharedAcc.Nvrs;
        }

        public INvr GetNvr(string NvrAddress)
        {
            var NvrHost = NvrAddress;
            var NvrPort = 38880;
            if (NvrAddress.Contains(":"))
            {
                var spltd = NvrAddress.Split(':');
                NvrHost = spltd[0];
                NvrPort = int.Parse(spltd[1]);
            }

            var nvr = SharedAcc.Nvrs.Where(p => p.Hostname == NvrHost && p.Port == NvrPort).DefaultIfEmpty(null).FirstOrDefault();

            if (nvr == null)
            {
                nvr = addNvr(NvrHost, NvrPort);
            }

            if (nvr != null && !nvr.Authenticated)
            {
                AvigilonDotNet.LoginResult loginResult = nvr.Login("administrator", "c0t0n3t3");

                if (loginResult != AvigilonDotNet.LoginResult.Successful)
                {
                    Console.WriteLine(("An error occurred while logging in to the NVR: " + loginResult.ToString()));
                }
                System.Threading.Thread.Sleep(200);


            }

            return nvr;
        }

        public INvr addNvr(string host, int port = 38880)
        {
            System.Net.IPAddress address;


            if (!System.Net.IPAddress.TryParse(host, out address))
            {
                Console.WriteLine("Invalid ADDRESS!");
                return null;
            }

            Console.WriteLine(("connecting " + host + ":" + port.ToString() + "..."));
            AvigilonDotNet.AvgError result = m_sharedAcc.AddNvr(host,port);

            if (AvigilonDotNet.ErrorHelper.IsError(result))
            {
                Console.WriteLine("An error occurred while adding the NVR.");
                return null;
            }

            System.Threading.Thread.Sleep(500);

            var nvr = m_sharedAcc.GetNvr(address);
            while (nvr == null)
            {
                System.Threading.Thread.Sleep(500);
                nvr = m_sharedAcc.GetNvr(address);
            }

            System.Threading.Thread.Sleep(200);
            

            return nvr;
        }



        public static Bitmap MakeBitmapFromImageRaw(AvigilonDotNet.IFrameImageRaw rawFrame)
        {
            Rectangle imageRect = new Rectangle(0, 0, rawFrame.Size.Width, rawFrame.Size.Height);

            switch (rawFrame.DefaultPixelFormat)
            {
                case AvigilonDotNet.PixelFormat.Gray8:
                    {
                        Bitmap retVal = new Bitmap(
                            rawFrame.Size.Width,
                            rawFrame.Size.Height,
                            System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

                        // Define palette for 8bit grayscale
                        System.Drawing.Imaging.ColorPalette pal = retVal.Palette;
                        for (int ix = 0; ix < pal.Entries.Length; ++ix)
                        {
                            pal.Entries[ix] = System.Drawing.Color.FromArgb(ix, ix, ix);
                        }
                        retVal.Palette = pal;

                        System.Drawing.Imaging.BitmapData bitmapData = retVal.LockBits(
                            imageRect,
                            System.Drawing.Imaging.ImageLockMode.WriteOnly,
                            System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

                        byte[] byteArray = rawFrame.GetAsArray(
                            AvigilonDotNet.PixelFormat.Gray8,
                            (ushort)bitmapData.Stride);

                        System.Runtime.InteropServices.Marshal.Copy(
                            byteArray,
                            0,
                            bitmapData.Scan0,
                            byteArray.Length);
                        retVal.UnlockBits(bitmapData);

                        return retVal;
                    }

                case AvigilonDotNet.PixelFormat.RGB24:
                    {
                        Bitmap retVal = new Bitmap(
                            rawFrame.Size.Width,
                            rawFrame.Size.Height,
                            System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                        System.Drawing.Imaging.BitmapData bitmapData = retVal.LockBits(
                            imageRect,
                            System.Drawing.Imaging.ImageLockMode.WriteOnly,
                            System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                        byte[] byteArray = rawFrame.GetAsArray(
                            AvigilonDotNet.PixelFormat.RGB24,
                            (ushort)bitmapData.Stride);

                        System.Runtime.InteropServices.Marshal.Copy(
                            byteArray,
                            0,
                            bitmapData.Scan0,
                            byteArray.Length);
                        retVal.UnlockBits(bitmapData);

                        return retVal;
                    }

                case AvigilonDotNet.PixelFormat.RGB32:
                    {
                        Bitmap retVal = new Bitmap(
                            rawFrame.Size.Width,
                            rawFrame.Size.Height,
                            System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                        System.Drawing.Imaging.BitmapData bitmapData = retVal.LockBits(
                            imageRect,
                            System.Drawing.Imaging.ImageLockMode.WriteOnly,
                            System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                        byte[] byteArray = rawFrame.GetAsArray(
                            AvigilonDotNet.PixelFormat.RGB32,
                            (ushort)bitmapData.Stride);

                        System.Runtime.InteropServices.Marshal.Copy(
                            byteArray,
                            0,
                            bitmapData.Scan0,
                            byteArray.Length);
                        retVal.UnlockBits(bitmapData);

                        return retVal;
                    }

                default:
                    return null;
            }
        }

        

    }
}