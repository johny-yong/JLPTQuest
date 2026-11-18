namespace JLPTQuest
{
    //"★★☆" with earned stars in vermilion and missed ones faint
    public static class StarText
    {
        public static string Line(bool[] earned)
        {
            string on = InkColors.ToTag(InkColors.Vermilion);
            string off = InkColors.ToTag(InkColors.InkFaint);
            string text = string.Empty;
            foreach (bool star in earned)
                text += "<color=" + (star ? on : off) + ">" + (star ? "★" : "☆") + "</color>";
            return text;
        }
    }
}
