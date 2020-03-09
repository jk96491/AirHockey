using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComMallet : MonoBehaviour
{

    public enum ActionState : int
    {
        NONE = -1,
        IDLE,
        CHASE,
        RETURN,
        MAX
    }

    [SerializeField]
    private Transform puckTrans = null;
    [SerializeField]
    private Transform startPointTrans = null;
    [SerializeField]
    private Puck puckInfo = null;

    [Header("[mallet 정보]")]
    [SerializeField]
    private Transform trans = null;
    [SerializeField]
    UITexture texture = null;

    [Header("[Block 정보]")]
    [SerializeField]
    private Transform leftWall = null;
    [SerializeField]
    private Transform rightWall = null;
    [SerializeField]
    private Transform topWall = null;
    [SerializeField]
    private Transform bottomWall = null;

    private int radius = 0;
    public int Radius { get { return radius; } }

    private float malletSpeed = 1150f / 2;

    [SerializeField]
    private ActionState curState = ActionState.NONE;

    private int puckRadius = 0;

    public void Init(int puckRadius_)
    {
        if(null != texture)
            radius = texture.width / 2;

        puckRadius = puckRadius_;
    }

    public void UpdateMallet(float Elapesd_)
    {
        MalletAction(Elapesd_);
    }

    private void ChangeState(ActionState state_)
    {
        if (curState == state_)
            return;

        curState = state_;

        if (state_ == ActionState.IDLE)
            idleFlowTime = 0f;
    }


    float idleFlowTime = 0f;

    private void MalletAction(float Elapesd_)
    {
        switch (curState)
        {
            case ActionState.CHASE:
                float dis = ChaseAction(Elapesd_);

                if (dis <= radius + puckRadius)
                    ChangeState(ActionState.RETURN);

                break;
            case ActionState.RETURN:
                dis = ReturnAction(Elapesd_);

                if (dis <= radius)
                    ChangeState(ActionState.IDLE);
                break;
            case ActionState.IDLE:
                ActionIdle(Elapesd_);

                idleFlowTime += Elapesd_;

                if(idleFlowTime >= 2f && puckInfo.CurDir == Puck.Direction.COM)
                    ChangeState(ActionState.CHASE);
                break;
        }
    }

    float moveIdleX = -1f;
    float moveIdleY = -1f;
    public void ActionIdle(float Elapesd_)
    {
        if(trans.localPosition.x <= -170)
            moveIdleX = 1;
        if (trans.localPosition.x >= 170)
            moveIdleX = -1;

        trans.localPosition += Vector3.right * Elapesd_ * malletSpeed * 0.5f * moveIdleX;

        if (trans.localPosition.y >= 560)
            moveIdleY = -1f;
        if(trans.localPosition.y <= 440)
            moveIdleY = 1f;

        trans.localPosition += Vector3.up * Elapesd_ * malletSpeed * 0.25f * moveIdleY;
    }

    public float ReturnAction(float Elapesd_)
    {
        return MoveToTarget(Elapesd_, startPointTrans.localPosition);
    }

    public float ChaseAction(float Elapesd_)
    {
        return MoveToTarget(Elapesd_, puckTrans.localPosition + new Vector3(0 , 30, 0));
    }

    public float MoveToTarget(float Elapesd_, Vector3 targetTransPos)
    {
        if (null == trans)
            return 0f;

        float dis = Vector3.Distance(targetTransPos, trans.localPosition);
        Vector3 vec = (targetTransPos - trans.localPosition).normalized;

        trans.localPosition += vec * Elapesd_ * malletSpeed * 1.15f;

        return dis;
    }

    public void ReachCenter(Puck.Direction dir)
    {
        if(dir == Puck.Direction.COM)
        {
            if (curState == ActionState.IDLE || curState == ActionState.NONE)
                ChangeState(ActionState.CHASE);
        }
        else
        {
            ChangeState(ActionState.RETURN);
        }
    }
}
