namespace Mackiloha.Render
{
    public interface IRenderObject : IMiloObject { }

    public abstract class RenderObject : MiloObject, IRenderObject
    {
        public override string Type => "Render";
    }
}
