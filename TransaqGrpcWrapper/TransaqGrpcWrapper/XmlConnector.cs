using System;
using System.Runtime.InteropServices;

namespace Firelib
{
    internal static class XmlConnector
    {
        public static string path;

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

            return ConnectorInitialize(path, 3);
        }


        //--------------------------------------------------------------------------------
        public static CommandStatusMsg ConnectorSendCommand(string command)
        {
            var pData = MarshalUtf8.StringToHGlobalUtf8(command);
            var msg= ptrToDescr(SendCommand(pData));
            Marshal.FreeHGlobal(pData);
            return new CommandStatusMsg
            {
                Msg = $"{msg}"
            };
        }

        private static string ptrToDescr(IntPtr pResult)
        {
            var result = MarshalUtf8.PtrToStringUtf8(pResult);
            FreeMemory(pResult);
            return result;
        }


        public static bool ConnectorInitialize(string path, short logLevel)
        {
            var pPath = MarshalUtf8.StringToHGlobalUtf8(path);
            var (msg, success) = ptrToDescr(Initialize(pPath, logLevel));
            Marshal.FreeHGlobal(pPath);
            Console.Out.WriteLine(!success ? $"failed to init connector {msg}" : $"Initialize() OK {msg}");
            return success;
        }


        public static void ConnectorUnInitialize()
        {
            var (msg, success) = ptrToDescr(UnInitialize());
            Console.Out.WriteLine(!success ? $"uninit failed {msg}" : $"uninit OK {msg}");
        }


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