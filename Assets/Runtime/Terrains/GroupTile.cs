using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName ="2D/Tiles/Group Tile")]
public class GroupTile : RuleTile<GroupTile.Neighbor> {
    public string GroupName;

    public class Neighbor : RuleTile.TilingRule.Neighbor {

    }

    public override bool RuleMatch(int neighbor, TileBase tile) {
        if(tile is RuleOverrideTile ruleOverrideTile) {
            tile = ruleOverrideTile.m_InstanceTile;
        }

        switch (neighbor) {
        case Neighbor.This: {
            GroupTile groupTile = tile as GroupTile;
            return groupTile && groupTile.GroupName == this.GroupName;
        }
        case Neighbor.NotThis: {
            GroupTile groupTile = tile as GroupTile;
            return !groupTile || groupTile.GroupName != this.GroupName;
        }
        }
        return base.RuleMatch(neighbor, tile);
    }
}