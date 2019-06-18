
using System;
using System.Threading;
using System.Threading.Tasks;
using Firelib;
using Grpc.Core;

namespace GreeterClient
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            Console.Out.WriteLine("Starting");
            
            var channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

            var client = new TransaqConnector.TransaqConnectorClient(channel);


            while (true)
                try
                {
                    var msgInfos = client.connect(new Empty());
                    
                    var request = new Str
                    {
                        Txt = makeLoginCommand("TCNN10000", "zuDR3W", "tr1-demo5.finam.ru", "3939")
                    };

                    var loginCommandStatus = client.sendCommand(request);

                    Console.Out.WriteLine($" login command out: {loginCommandStatus}");

                    while (await msgInfos.ResponseStream.MoveNext(CancellationToken.None))
                    {
                        Console.Out.WriteLine("received");
                        Console.Out.WriteLine(msgInfos.ResponseStream.Current.Txt);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"exception throwen {e.Message} reconnecting");
                    Thread.Sleep(1000);
                }


            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }


        public static string makeLoginCommand(string sLogin, string sPassword, string ServerIP, string ServerPort)
        {
            return "<command id=\"connect\">" +
                   $"<login>{sLogin}</login>" +
                   $"<password>{sPassword}</password>" +
                   $"<host>{ServerIP}</host>" +
                   $"<port>{ServerPort}</port>" +
                   "<rqdelay>100</rqdelay>" +
                   "<session_timeout>1000</session_timeout> " +
                   "<request_timeout>1000</request_timeout>" +
                   "</command>";
        }
    }
}