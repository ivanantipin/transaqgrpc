transaq provide [gRPC](https://grpc.io/) interface to 
[TransaqConnector](https://www.finam.ru/howtotrade/tconnector/)
to be able to connect from different languages via TCP (remote procedure call) 


current wrapper works with 64 bit version of transaq.dll

description of service

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