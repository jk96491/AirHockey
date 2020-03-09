using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class MalletAgent : Agent {

    [Header("[Mallet 정보]")]
    [SerializeField]
    private Transform trans = null;
    [SerializeField]
    private UITexture texture;
    [SerializeField]
    private Transform comMalletTrans = null;
    [SerializeField]
    private Puck puckTrans = null;

    private int malletRadius = 0;

    [Header("[Block 정보]")]
    [SerializeField]
    private Transform leftWall = null;
    [SerializeField]
    private Transform rightWall = null;
    [SerializeField]
    private Transform topWall = null;
    [SerializeField]
    private Transform bottomWall = null;

    [SerializeField]
    private Vector3 moveVector = Vector3.zero;

    [SerializeField]
    Vector3 firsPos = Vector3.zero;

    float ElapesdTime = 0f;

    float puckSpeed = 500f;

    public void Init()
    {
        if (null != texture)
            malletRadius = texture.width / 2;

        if(null != trans)
            trans.localPosition = firsPos;
    }

    public override void CollectObservations()
    {
        AddVectorObs(comMalletTrans.localPosition);
        AddVectorObs(trans.localPosition);
        AddVectorObs(puckTrans.transform.localPosition);
        AddVectorObs(puckTrans.MoveVector);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
	{
        float x = vectorAction[0];
        float y = vectorAction[1];

        if (null != trans)
            trans.localPosition += new Vector3(x, y, 0) * puckSpeed * ElapesdTime;
    }

    public override void AgentReset()
    {

    }

    public override void AgentOnDone()
    {

    }

    public void UpdateMallet(float Elapesd_)
    {
        ElapesdTime = Elapesd_;
        SetBlock();
    }

    public void SetBlock()
    {
        if (null == trans)
            return;

        Vector3 curPos = trans.localPosition;
        Vector3 resetPos = curPos;

        if (curPos.x <= leftWall.localPosition.x + malletRadius)
        {
            resetPos = new Vector3(leftWall.localPosition.x + malletRadius, curPos.y, curPos.z);
        }
        if (curPos.x >= rightWall.localPosition.x - malletRadius)
        {
            resetPos = new Vector3(rightWall.localPosition.x - malletRadius, resetPos.y, resetPos.z);
        }
        if (curPos.y >= topWall.localPosition.y - malletRadius)
        {
            resetPos = new Vector3(resetPos.x, topWall.localPosition.y - malletRadius, resetPos.z);
        }
        if (curPos.y <= bottomWall.localPosition.y + malletRadius)
        {
            resetPos = new Vector3(resetPos.x, bottomWall.localPosition.y + malletRadius, resetPos.z);
        }

        trans.localPosition = resetPos;
    }

    public void HitMallet(float vecY)
    {
        if (vecY > 0)
            SetReward(0.5f);
        else
            SetReward(-0.8f);
    }
}
