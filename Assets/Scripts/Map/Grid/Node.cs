using UnityEngine;

public class Node
{
    public Vector2Int axial;
    public bool walkable;

    private Character occupiedCharacter;

    public Character OccupiedCharacter => occupiedCharacter;

    public bool IsOccupied => occupiedCharacter != null;

    public Node(Vector2Int axial, bool walkable)
    {
        this.axial = axial;
        this.walkable = walkable;
    }

    public void SetCharacter(Character character)
    {
        occupiedCharacter = character;
    }

    public void ClearCharacter()
    {
        occupiedCharacter = null;
    }
}
