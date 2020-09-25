using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Grpc.Core;
using Robotsystemcommunication;


public class UnityBrainServer : MonoBehaviour
{
    public List<RemoteAIRobotAgent> m_RemoteAgents = new List<RemoteAIRobotAgent>();

    [Space(10)]
    public int Port = 50052;

    private Server server;

    void Start()
    {
        StartServer();
    }

    private void StartServer()
    {
        BrainServerImpl brainServerImpl = new BrainServerImpl(ListToDict(m_RemoteAgents));

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

    private Dictionary<int, RemoteAIRobotAgent> ListToDict(List<RemoteAIRobotAgent> agentList)
    {
        Dictionary<int, RemoteAIRobotAgent> agentDict = new Dictionary<int, RemoteAIRobotAgent>(); 

        foreach (var agent in agentList)
        {
            agentDict.Add(agent.m_ArucoMarkerID, agent);
        }
        return agentDict;
    }

    class BrainServerImpl : BrainServer.BrainServerBase
    {
        private Dictionary<int, RemoteAIRobotAgent> agentDict;

        public BrainServerImpl(Dictionary<int, RemoteAIRobotAgent> agentDict) : base()
        {
            this.agentDict = agentDict;
        }

        public override Task<BrainActionResponse> GetAction(BrainActionRequest req, ServerCallContext context)
        {
            try
            {             
                foreach (var actionReq in req.Observations)
                {
                    var lowerObsList = new List<float>();
                    lowerObsList.AddRange(actionReq.LowerObservations);
                    var upperObsList = new List<float>();
                    upperObsList.AddRange(actionReq.UpperObservations);
                    // Set observations to remote agent.
                    agentDict[actionReq.ArucoMarkerID].SetObservations(lowerObsList.ToArray(), upperObsList.ToArray());
                    agentDict[actionReq.ArucoMarkerID].RequestDecision();
                }

                var brainActionRes = new BrainActionResponse();
                foreach (KeyValuePair<int, RemoteAIRobotAgent> agent in agentDict)
                {
                    int action = -1;
                    while(action < 0)
                    {
                        action = agent.Value.GetDecidedAction();
                        if (action != -1) break;
                        Thread.Sleep(3);
                    }
                    var newAction = new RobotAction(){Action = action, ArucoMarkerID = agent.Key};
                    brainActionRes.Actions.Add(newAction);
                }

                // Send remote agents action back.
                return Task.FromResult(brainActionRes);
            }
            catch (Exception error)
            {
                Debug.Log(error);
                var brainActionResFail = new BrainActionResponse();
                foreach (KeyValuePair<int, RemoteAIRobotAgent> agent in agentDict)
                {
                    var newAction = new RobotAction(){Action = 0, ArucoMarkerID = agent.Key};
                    brainActionResFail.Actions.Add(newAction);
                }
                return Task.FromResult(brainActionResFail);
            }
        }
    }
}
