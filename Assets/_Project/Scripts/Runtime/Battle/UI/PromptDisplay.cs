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

        /*
            The instruction column as a cue under the sentence, like "(not a doctor)".
            Any question where a wrong choice could also make a real sentence needs one, or the player is guessing.
        */
        public static string Cue(string instruction)
        {
            return string.IsNullOrWhiteSpace(instruction) ? string.Empty : "(" + instruction.Trim() + ")";
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
