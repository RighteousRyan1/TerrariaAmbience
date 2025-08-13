namespace TerrariaAmbience.Content.InventorySounds;
public class ItemChangeTracker {
    private int _lastMouseItemType;
    private int _lastHeldItemType;

    public bool HasMouseItemChanged(int currentType) {
        bool changed = _lastMouseItemType != currentType;
        _lastMouseItemType = currentType;
        return changed;
    }

    public bool HasHeldItemChanged(int currentType) {
        bool changed = _lastHeldItemType != currentType;
        _lastHeldItemType = currentType;
        return changed;
    }
}
