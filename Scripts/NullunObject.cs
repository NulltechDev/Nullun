using Godot;

namespace Nullun.Scripts;

public partial class NullunObject : Node2D
{
    public override void _Ready()
    {
        InitContent();
    }
    
    protected virtual void InitContent()
    {
        Declare();
        Event();
    }

    protected virtual void Declare()
    {
    }

    protected virtual void Event()
    {
    }
}