namespace Mackiloha.DTB;

public class FloatItem : DTBItem
{
    public FloatItem() : this(0.0f)
    {

    }

    public FloatItem(float value)
    {
        Float = value;
    }

    public float Float { get; set; }

    public int NumericValue
    {
        get { return 0x01; }
    }
}
