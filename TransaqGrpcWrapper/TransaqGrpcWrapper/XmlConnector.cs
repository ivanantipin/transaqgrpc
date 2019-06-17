using System;
using System.Runtime.InteropServices;

// ReSharper disable once IdentifierTypo
namespace Firelib
{
    internal static class XmlConnector
    {
        public static string Path;

        public static bool Init(Func<string, string> callback)
        {
            CallBackDelegate predicate = data =>
            {
                var result = MarshalUtf8.PtrToStringUtf8(data);
                FreeMemory(data);
                callback(result);
                return true;
            };
            var CallbackHandle = GCHandle.Alloc(predicate);

            if (!SetCallback(predicate)) throw new Exception("Cant set callback");

            return ConnectorInitialize(Path, 3);
        }


        //--------------------------------------------------------------------------------
        public static Str ConnectorSendCommand(string command)
        {
            var pData = MarshalUtf8.StringToHGlobalUtf8(command);
            var msg = PtrToDescr(SendCommand(pData));
            Marshal.FreeHGlobal(pData);
            return new Str
            {
                Txt = msg
            };
        }

        private static string PtrToDescr(IntPtr pResult)
        {
            if (IntPtr.Zero == pResult)
            {
                return null;
            }
            var result = MarshalUtf8.PtrToStringUtf8(pResult);
            FreeMemory(pResult);
            return result;
        }


        private static bool ConnectorInitialize(string path, short logLevel)
        {
            var pPath = MarshalUtf8.StringToHGlobalUtf8(path);
            var msg = PtrToDescr(Initialize(pPath, logLevel));
            Marshal.FreeHGlobal(pPath);
            Console.Out.WriteLine(msg != null ? $"failed to init connector {msg}" : $"Initialize() OK");
            return msg == null;
        }


// fixme do we need this?        public static void ConnectorUnInitialize()
//        {
//            var msg = PtrToDescr(UnInitialize());
//            Console.Out.WriteLine(!success ? $"uninit failed {msg}" : $"uninit OK {msg}");
//        }


        //--------------------------------------------------------------------------------
        // файл библиотеки TXmlConnector.dll должен находиться в одной папке с программой

        [DllImport("txmlconnector64.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool SetCallback(CallBackDelegate pCallback);

        //[DllImport("txmlconnector.dll", CallingConvention = CallingConvention.StdCall)]
        //private static extern bool SetCallbackEx(CallBackExDelegate pCallbackEx, IntPtr userData);

        [DllImport("txmlconnector64.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr SendCommand(IntPtr pData);

        [DllImport("txmlconnector64.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool FreeMemory(IntPtr pData);

        [DllImport("txmlConnector64.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr Initialize(IntPtr pPath, int logLevel);

        [DllImport("txmlConnector64.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr UnInitialize();

        [DllImport("txmlConnector64.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr SetLogLevel(int logLevel);

        private delegate bool CallBackDelegate(IntPtr pData);

        //--------------------------------------------------------------------------------
    }
}