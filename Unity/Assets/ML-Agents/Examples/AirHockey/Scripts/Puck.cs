using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puck : MonoBehaviour
{
    public enum Direction
    {
        NONE = -1,
        PLAYER,
        COM
    }

    [SerializeField]
    private Transform comMalletTrans = null;
    [SerializeField]
    private Transform agentMalletTrans = null;
    private int malletRadius = 0;

    [Header("[Puck 정보]")]
    [SerializeField]
    private Transform trans = null;
    [SerializeField]
    UITexture texture = null;
    private int puckRadius = 0;
    public int PuckRadius { get { return puckRadius; } }
    public Transform Trans { get { return trans; } }

    [Header("[Block 정보]")]
    [SerializeField]
    private Transform leftWall = null;
    [SerializeField]
    private Transform rightWall = null;
    [SerializeField]
    private Transform topWall = null;
    [SerializeField]
    private Transform bottomWall = null;

    [Header("[Goal 정보]")]
    [SerializeField]
    private Transform goalLeft = null;
    [SerializeField]
    private Transform goalRight = null;

    [SerializeField]
    private Vector3 moveVector = Vector3.zero;
    public Vector3 MoveVector { get { return moveVector; } }

    [SerializeField]
    private Transform center = null;

    float puckSpeed = 650;

    Vector3 startPos = Vector3.zero;

    public delegate void ReachCenter(Direction dir);
    public ReachCenter ReachCenterDel;

    public delegate void GoalEvent(Direction dir);
    public GoalEvent GoalEventDel;

    public delegate void HitMalletEvent(float vecY);
    public HitMalletEvent HitMalletEventDel;

    private Direction curDir = Direction.NONE;
    public Direction CurDir { get { return curDir;} }

    public void Init(int malletRadius_)
    {
        if (null != texture)
            puckRadius = texture.width / 2;

        malletRadius = malletRadius_;

        ResetPuck();
    }

    public void UpdatePuck(float Elapesd_)
    {
        MovePuck(Elapesd_);
        SetBlock();
        CheckCollisionMallet();
        SetDirection();
    }

    public void SetDirection()
    {
        if (curDir == Direction.PLAYER)
        {
            if (trans.localPosition.y >= center.localPosition.y)
            {
                if (null != ReachCenterDel)
                    ReachCenterDel(Direction.COM);

                curDir = Direction.COM;
            }
        }
        else if (curDir == Direction.COM)
        {
            if (trans.localPosition.y <= center.localPosition.y)
            {
                if (null != ReachCenterDel)
                    ReachCenterDel(Direction.PLAYER);

                curDir = Direction.PLAYER;
            }
        }
    }

    public void MovePuck(float Elapesd_)
    {
        if (null == trans)
            return;

        trans.localPosition += moveVector * Elapesd_ * puckSpeed;
    }

    public void SetMoveVector(Vector3 moveVec_)
    {
        moveVector = moveVec_;
    }

    public void SetBlock()
    {
        if (null == trans)
            return;

        Vector3 curPos = trans.localPosition;
        Vector3 resetPos = curPos;

        if (curPos.x <= leftWall.localPosition.x + puckRadius)
        {
            resetPos = new Vector3(leftWall.localPosition.x + puckRadius, curPos.y, curPos.z);
            moveVector = new Vector3(-moveVector.x, moveVector.y, moveVector.z);
        }
        if (curPos.x >= rightWall.localPosition.x - puckRadius)
        {
            resetPos = new Vector3(rightWall.localPosition.x - puckRadius, curPos.y, curPos.z);
            moveVector = new Vector3(-moveVector.x, moveVector.y, moveVector.z);
        }
        if (curPos.y >= topWall.localPosition.y - puckRadius)
        {
            bool isGoal = isGoalPos(curPos);

            if(false == isGoal)
            {
                resetPos = new Vector3(curPos.x, topWall.localPosition.y - puckRadius, curPos.z);
                moveVector = new Vector3(moveVector.x, -moveVector.y, moveVector.z);
            }
            else
            {
                if (null != GoalEventDel)
                    GoalEventDel(Direction.PLAYER);

                return;
            }
        }
        if (curPos.y <= bottomWall.localPosition.y + puckRadius)
        {
            bool isGoal = isGoalPos(curPos);

            if (false == isGoal)
            {
                resetPos = new Vector3(curPos.x, bottomWall.localPosition.y + puckRadius, curPos.z);
                moveVector = new Vector3(moveVector.x, -moveVector.y, moveVector.z);
            }
            else
            {
                if (null != GoalEventDel)
                    GoalEventDel(Direction.PLAYER);

                return;
            }
        }


        trans.localPosition = resetPos;
    }

    private bool isGoalPos(Vector3 pos_)
    {
        bool result = false;

        if (pos_.x >= goalLeft.localPosition.x && pos_.x <= goalRight.localPosition.x)
            result = true;

        return result;
    }

    public void CheckCollisionMallet()
    {
        int combineRadius = malletRadius + puckRadius;

        if (null != comMalletTrans)
        {
            Vector3 vec = (trans.localPosition - comMalletTrans.localPosition).normalized;

            float dis = Vector3.Distance(comMalletTrans.localPosition, trans.localPosition);

            if (dis <= combineRadius)
            {
                moveVector = vec;
            }
        }

        if (null != agentMalletTrans)
        {
            Vector3 vec = (trans.localPosition - agentMalletTrans.localPosition).normalized;

            float dis = Vector3.Distance(agentMalletTrans.localPosition, trans.localPosition);

            if (dis <= combineRadius)
            {
                moveVector = vec;

                if (null != HitMalletEventDel)
                    HitMalletEventDel(vec.y);
            }
        }
    }

    public void ResetPuck()
    {
        if (null == trans)
            return;

        trans.localPosition = startPos;

        float vecX = Random.Range(-0.5f, 0.5f);
        float vecY = Random.Range(-0.5f, 0.5f);

        if (vecX == 0 || vecY == 0)
        {
            vecX = 0.5f * Random.Range(0, 100) % 2 == 0 ? 1 : -1;
            vecY = 0.5f * Random.Range(0, 100) % 2 == 0 ? 1 : -1;
        }

        if (vecY > 0)
            curDir = Direction.COM;
        else
            curDir = Direction.PLAYER;

        if (null != ReachCenterDel)
            ReachCenterDel(curDir);

        moveVector = new Vector3(vecX, vecY, 0);
    }
}
