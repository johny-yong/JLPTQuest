namespace JLPTQuest
{
    //How a prompt looks on screen: blanks drawn as a faint line, then filled in once answered
    public static class PromptDisplay
    {
        public static string WithBlanks(string prompt)
        {
            string blank = "<color=" + InkColors.ToTag(InkColors.InkFaint) + ">＿＿＿</color>";
            return (prompt ?? string.Empty).Replace(ContentText.Blank.ToString(), blank);
        }

        //Fills the first blank with the answer in vermilion, any other blanks stay empty
        public static string WithAnswer(string prompt, string answer)
        {
            prompt = prompt ?? string.Empty;
            int index = prompt.IndexOf(ContentText.Blank);
            if (index < 0) return prompt;

            string filled = "<color=" + InkColors.ToTag(InkColors.Vermilion) + ">" + answer + "</color>";
            string result = prompt.Substring(0, index) + filled + prompt.Substring(index + 1);
            return WithBlanks(result);
        }
    }
}
