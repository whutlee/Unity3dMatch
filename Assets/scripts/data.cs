using UnityEngine;
using System.Collections.Generic;

public	enum FaceAnim
	{
		enmHit,
		enmNormal,
		enmTouch,
		enmDrag,
		enmNum
	}
public enum JewelType
{
	enNone,
	enIce,
	enFire,
	enStorm,
	enAttack,
	enDummy,
	enRecovery,
	enNum,
};

public enum GameResult
{
	enNone,
	enWin,
	enLose
}
public enum GameTurn
{
	enNone,
	enYourTurn,
	enAITurn
}
public enum JewelState
{
	enIdle,
	enEffecting,
	enTouched,
	enSelected
}
public struct PairJewel
{
	public jewel l;
	public jewel r;
}
public struct ClearInfo
{
	public List<GridPos> position;
	public JewelType type;
	public float deltaTime;
	public float ClearTime;
	public GameTurn owner;
};
public struct GridPos
{
	public int x;
	public int y;
	
	 public static bool operator ==(GridPos a, GridPos b)
    {
        return (a.x == b.x) && (a.y == b.y);
    }

    public static bool operator !=(GridPos a, GridPos b)
    {
        return (a.x != b.x) || (a.y != b.y);
    }
	
	public override bool Equals(object obj)
    {
        GridPos tmp = (GridPos)obj;
        if (this == tmp)
            return true;
        else
            return false;
    }
	 public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
