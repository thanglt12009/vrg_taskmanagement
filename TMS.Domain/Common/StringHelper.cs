namespace TMS.Domain.Common
{
    public static class StringHelper
    {
        public static string CutString(string inputText, int stringLength, string replaceChar)
        {
            var outputString = inputText;

            if (inputText == null) return string.Empty;

            if(inputText.Length > stringLength)
            {
                outputString = inputText.Substring(0, stringLength) + replaceChar;
            }

            return outputString;
        }
    }
}
