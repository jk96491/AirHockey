using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : MonoBehaviour
{
    [SerializeField]
    private ComMallet comMallet = null;
    [SerializeField]
    private Puck puck = null;
    [SerializeField]
    private MalletAgent agent = null;

    public void Start()
    {
        if (null != puck)
        {
            puck.GoalEventDel += GoalEvent;
            puck.ReachCenterDel += comMallet.ReachCenter;
            puck.HitMalletEventDel += agent.HitMallet;
        }
        ResetData();
    }

    public void ResetData()
    {
        comMallet.Init(puck.PuckRadius);

        if (null != puck)
        {
            puck.Init(comMallet.Radius);
               
            comMallet.Init(puck.PuckRadius);
            puck.ResetPuck();
        }

        if (null != agent)
            agent.Init();
    }

    public void FixedUpdate()
    {
        float time = Time.deltaTime;

        if (null != puck)
            puck.UpdatePuck(time);

        if (null != agent)
            agent.UpdateMallet(time);

        if (null != comMallet)
            comMallet.UpdateMallet(time);
    }

    public void GoalEvent(Puck.Direction Who)
    {
        if (Who == Puck.Direction.PLAYER)
            agent.SetReward(10f);
        else
            agent.SetReward(-10f);

        agent.Done();

        ResetData();

    }
}
