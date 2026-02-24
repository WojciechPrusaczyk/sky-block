using UnityEngine;

/// <summary>
/// Example implementation of a world block behaviour for a spruce tree.
/// Inherit from <see cref="BlockBehaviour"/> and override hooks to define
/// block-specific interactions in one place.
/// </summary>
public class Boulder : BlockBehaviour
{
    #region Placement

    /// <summary>
    /// Called once after this block is spawned on the tilemap.
    /// </summary>
    /// <param name="placedByItem">Item used to place this block. Can be null.</param>
    public override void OnPlaced(Item placedByItem)
    {
        // Example: initialize random tree variant, growth timer, etc.
    }

    /// <summary>
    /// Called right before the block is removed from the world.
    /// </summary>
    protected override void OnDestroyed()
    {

    }

    #endregion

    #region Interaction

    /// <summary>
    /// Called on left click before default hit logic.
    /// Return true to fully handle input and skip default damage processing.
    /// Return false to let the base flow continue with <see cref="BlockBehaviour.Hit(Item)"/>.
    /// </summary>
    /// <param name="usedByItem">Currently selected item used by player. Can be null.</param>
    public override bool OnUse(Item usedByItem)
    {
        // Example: open custom UI, start harvesting state, etc.
        return false;
    }

    /// <summary>
    /// Called on right click.
    /// </summary>
    /// <param name="usedByItem">Currently selected item used by player. Can be null.</param>
    public override bool OnAltUse(Item usedByItem)
    {
        // Example: shake tree, collect sap, rotate variant, etc.
        return false;
    }

    #endregion

    #region Combat

    /// <summary>
    /// Called when this block receives hit damage.
    /// Allows block-specific damage adjustments (e.g. resistances, weak spots).
    /// </summary>
    /// <param name="hitByItem">Item used to hit this block. Can be null.</param>
    /// <param name="damage">Damage calculated by base logic.</param>
    protected override float OnHit(Item hitByItem, float damage)
    {
        // Example: reduce damage in rain, increase for critical tools, etc.
        return damage;
    }

    /// <summary>
    /// Label used in default HP logs from <see cref="BlockBehaviour"/>.
    /// </summary>
    protected override string GetHitLogName()
    {
        return "Boulder";
    }

    #endregion
}