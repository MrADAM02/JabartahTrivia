using System.Text.RegularExpressions;

namespace Jabartah.Trivia.Domain.Top100Game;

public static class Top100AnswerNormalizer
{
    public static string Normalize(string input)
    {
        var chars = input.Trim().Select(ch => ch switch
        {
            'أ' or 'إ' or 'آ' => 'ا',
            'ة' => 'ه',
            'ى' => 'ي',
            _ => ch
        });
        return Regex.Replace(new string(chars.ToArray()), @"\s+", " ");
    }
}
