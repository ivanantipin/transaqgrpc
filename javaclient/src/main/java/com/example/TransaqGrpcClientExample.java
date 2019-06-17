package com.example;

import com.firelib.*;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;

import java.util.Iterator;
import java.util.concurrent.TimeUnit;
import java.util.logging.Logger;

public class TransaqGrpcClientExample {
    private static final Logger logger = Logger.getLogger(TransaqGrpcClientExample.class.getName());

    private final ManagedChannel channel;
    private final TransaqConnectorGrpc.TransaqConnectorBlockingStub blockingStub;

    /** Construct client connecting to HelloWorld server at {@code host:port}. */
    public TransaqGrpcClientExample(String host, int port) {
        this(ManagedChannelBuilder.forAddress(host, port)
                // Channels are secure by default (via SSL/TLS). For the example we disable TLS to avoid
                // needing certificates.
                .usePlaintext()
                .build());
    }

    TransaqGrpcClientExample(ManagedChannel channel) {
        this.channel = channel;
        blockingStub = TransaqConnectorGrpc.newBlockingStub(channel);

    }

    public void shutdown() throws InterruptedException {
        channel.shutdown().awaitTermination(5, TimeUnit.SECONDS);
    }

    static String getLoginCommand(String login, String passwd, String host, String port){

        return "<command id=\"connect\">" +
                "<login>"+ login + "</login>" +
                "<password>"+ passwd + "</password>" +
                "<host>"+ host + "</host>" +
                "<port>"+ port + "</port>" +
                "<rqdelay>100</rqdelay>" +
                "<session_timeout>1000</session_timeout> " +
                "<request_timeout>1000</request_timeout>" +
                "</command>";
    }

    /**
     * Greet server. If provided, the first element of {@code args} is the name to use in the
     * greeting.
     */
    public static void main(String[] args) throws Exception {
        TransaqGrpcClientExample client = new TransaqGrpcClientExample("localhost", 50051);


        while (true){
            try {

                Str response = client.blockingStub.sendCommand(Str.newBuilder().setTxt(getLoginCommand("TCNN9977", "v8GuG5", "tr1-demo5.finam.ru", "3939")).build());

                System.out.println("login command response:" + response);

                Iterator<Str> messages = client.blockingStub.connect(Empty.newBuilder().build());

                //continuous messages, this call will generally block till the end
                messages.forEachRemaining(str->{
                    System.out.println("server message" + str);
                });

            }catch (Exception e){
                e.printStackTrace();
                Thread.sleep(5000);
            }
        }
    }
}