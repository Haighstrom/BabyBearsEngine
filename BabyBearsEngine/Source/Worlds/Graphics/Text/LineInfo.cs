namespace BabyBearsEngine.Worlds.Graphics.Text;

internal record LineInfo(StyledChar[] Chars, int StartIndex, int EndIndex)
{
    internal string Content
    {
        get
        {
            char[] chars = new char[Chars.Length];
            for (int i = 0; i < Chars.Length; i++)
            {
                chars[i] = Chars[i].Char;
            }
            return new string(chars);
        }
    }
}
