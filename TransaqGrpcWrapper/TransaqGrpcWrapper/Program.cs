using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace Firelib
{
    public class TransaqConnectorImpl : TransaqConnector.TransaqConnectorBase
    {
        
        private readonly ConcurrentDictionary<IServerStreamWriter<Str>,BlockingCollection<string>> dic = new ConcurrentDictionary<IServerStreamWriter<Str>, BlockingCollection<string>>();

        public void onMsg(string str)
        {
            try
            {
                str = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(str));
                Console.Out.WriteLine($"received from transaq: {str}");
                foreach (var blockingCollection in dic.Values)
                {
                    blockingCollection.Add(str);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override Task connect(Empty request, IServerStreamWriter<Str> responseStream, ServerCallContext context)
        {

            dic[responseStream] = new BlockingCollection<string>();
            
            return Task.Run(() =>
            {
                try
                {
                    while (true)
                    
                    {
                        var take = dic[responseStream].Take();
                        Console.Out.WriteLine($"pushed to client: {take}");
                        responseStream.WriteAsync(new Str {Data = take});
                    }

                }
                catch (Exception e)
                {
                    BlockingCollection<string> tmp;
                    dic.Remove(responseStream, out tmp);
                    Console.WriteLine(e);
                }
            });
        }

        public override Task<CommandStatusMsg> sendCommand(Command request, ServerCallContext context)
        {
            try
            {
                Console.Out.WriteLine($"sending command {request}");
                var ret = XmlConnector.ConnectorSendCommand(request.Txt);

                Console.Out.WriteLine($"command response: {ret}");
                return Task.FromResult(ret);
            }
            catch (Exception e)
            {
                return Task.FromResult(new CommandStatusMsg
                {
                    Status = CommandStatus.Fail
                });
            }
        }
    }

    internal class Program
    {
        private const int Port = 50051;

        public static void Main(string[] args)
        {
            XmlConnector.path = args.Length == 0 ? Directory.GetCurrentDirectory() : args[0];

            var transaqConnectorImpl = new TransaqConnectorImpl();
            var server = new Server
            {
                Services = {TransaqConnector.BindService(transaqConnectorImpl)},
                Ports = {new ServerPort("0.0.0.0", Port, ServerCredentials.Insecure)}
            };

            XmlConnector.Init(str =>
            {
                transaqConnectorImpl.onMsg(str);
                return str;
            });

            server.Start();

            Console.WriteLine("Greeter server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}