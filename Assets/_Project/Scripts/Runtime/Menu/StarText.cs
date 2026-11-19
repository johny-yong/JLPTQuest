namespace JLPTQuest
{
    //"★★☆" with earned stars in gold leaf and missed ones faint
    public static class StarText
    {
        public static string Line(bool[] earned)
        {
            string on = InkColors.ToTag(InkColors.GoldLight);
            string off = InkColors.ToTag(InkColors.StarOff);
            string text = string.Empty;
            foreach (bool star in earned)
                text += "<color=" + (star ? on : off) + ">" + (star ? "★" : "☆") + "</color>";
            return text;
        }
    }
}
