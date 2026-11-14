namespace ClipX2.TextEngine
{
    public interface ITextTransform
    {
        string TransformPlain(string input);
        void TransformRtf(System.Windows.Forms.RichTextBox rtb);
    }
}
