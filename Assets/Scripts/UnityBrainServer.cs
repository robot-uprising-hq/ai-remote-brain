﻿using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Grpc.Core;
using Robotsystemcommunication;


public class UnityBrainServer : MonoBehaviour
{
    public RemoteAgent remoteAgent;

    [Space(10)]
    public int Port = 50052;

    private Server server;

    void Start()
    {
        StartServer();
    }

    private void StartServer()
    {
        BrainServerImpl brainServerImpl = new BrainServerImpl(remoteAgent);

        server = new Server
            {
                Services = { BrainServer.BindService(brainServerImpl) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
        server.Start();
        Debug.Log("\n=====\nServer started\n=====\n");
    }

    private void StopServer()
    {
        server.ShutdownAsync().Wait();
    }

    class BrainServerImpl : BrainServer.BrainServerBase
    {
        private RemoteAgent remoteAgent;

        public BrainServerImpl(RemoteAgent remoteAgent) : base()
        {
            this.remoteAgent = remoteAgent;
        }

        public override Task<BrainActionResponse> GetAction(BrainActionRequest req, ServerCallContext context)
        {
            var lowerObsList = new List<float>();
            lowerObsList.AddRange(req.LowerObservations);
            var upperObsList = new List<float>();
            upperObsList.AddRange(req.UpperObservations);

            // Set observations to remote agent.
            remoteAgent.SetObservations(lowerObsList.ToArray(), upperObsList.ToArray());
            remoteAgent.RequestDecision();

            int action = -1;
            while(action < 0)
            {
                Thread.Sleep(10); 
                action = remoteAgent.GetDecidedAction();
            }

            // Send remote agents action back.
            return Task.FromResult(new BrainActionResponse { Action = action });
        }
    }
}
