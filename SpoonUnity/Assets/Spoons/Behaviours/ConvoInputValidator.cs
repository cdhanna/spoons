using System.Collections.Generic;
using UnityEngine;

namespace Spoons.Behaviours
{
    [CreateAssetMenu(fileName = "Spoon Input Field Validator", menuName = "Spoon Input Field Validator")]
    public class ConvoInputValidator : TMPro.TMP_InputValidator
    {
        public static readonly HashSet<char> invalidChars = new HashSet<char>(new char[]
        {
            '"', '[', ']', '{', '}'
        });
        
        public override char Validate(ref string text, ref int pos, char ch)
        {
            if (invalidChars.Contains(ch))
            {
                // TODO: do some sort of "invalid" screen shake.
                return ch;
            }
            
            text += ch;
            pos++;
            return ch;
        }
    }
}