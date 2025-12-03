using Godot;

namespace Nullun.Resource.Scripts;

public partial class NullunObject : Node2D
{
    public override void _Ready()
    {
        base._Ready();
        InitContent();
    }
    private void InitContent()
    {
        Declare();
        Invoke();
    }

    protected virtual void Declare()
    {
    }

    protected virtual void Invoke()
    {
    }
}