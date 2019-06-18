### TransaqGrpcWrapper 

#### Description

[gRPC](https://grpc.io/) interface around 
[TransaqConnector](https://www.finam.ru/howtotrade/tconnector/)
to be able to connect from different languages via TCP (remote procedure call) and linux as well 

#### Modules

1. *tqgrpcserver* - server around txmlconnector64.dll
2. *client*
    1. *java* - java client grpc example
    2. *csharp* - c# client grpc example
3. protos/transaq.proto - service description, see below



#### Service proto description

```protobuf

syntax = "proto3";

option java_multiple_files = true;
option java_package = "com.firelib";
option java_outer_classname = "Transaq";

package firelib;


service TransaqConnector {

  // streaming messages from transaq api
  // you can connect from multiple applications and messages will be broadcasted
  rpc connect (Empty) returns (stream Str) {}

  // method to call command, returns string from transaqapi
  rpc sendCommand(Str) returns (Str){}
}

//just wrapper around string as grpc does not allow to use plain string in interface
message Str{
  string txt = 1;
}

message Empty {}


```

#### Starting server in Linux

server can be started with [Wine](https://www.winehq.org/) version 4.0 , Wine has to have .net installed ( should be prompted)

##### Example output:

```shell
ivan@fire:~/vbshare$ wine win7-x64/tqgrpcserver.exe 
Initialize() OK
Greeter server listening on port 50051
Press any key to stop the server...
```

#### Known issues

1. russian messages does not look good in console output
2. port is unprotected - hackers can break this from outside!! need to enable SSL in grpc