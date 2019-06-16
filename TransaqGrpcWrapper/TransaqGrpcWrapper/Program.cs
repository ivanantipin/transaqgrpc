// Copyright 2015 gRPC authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;

namespace Firelib
{
    public class TransaqConnectorImpl : TransaqConnector.TransaqConnectorBase
    {
        private readonly BlockingCollection<string> bcs = new BlockingCollection<string>();

        public void onMsg(string str)
        {
            try
            {
                Console.Out.WriteLine($"received from transaq: {str}");
                bcs.Add(str);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override Task connect(Empty request, IServerStreamWriter<Str> responseStream, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    var take = bcs.Take();
                    Console.Out.WriteLine($"pushed to client: {take}");
                    responseStream.WriteAsync(new Str {Data = take});
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
                Ports = {new ServerPort("localhost", Port, ServerCredentials.Insecure)}
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