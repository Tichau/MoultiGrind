using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public static class Helpers
    {
        public static void SetTextToSignedNumber(this Text text, Number value)
        {
            if (value == Number.Zero)
            {
                text.text = string.Empty;
            }
            else if (value > Number.Zero)
            {
                text.color = Color.green;
                text.text = value.ToString(true);
            }
            else
            {
                text.color = Color.red;
                text.text = value.ToString(true);
            }
        }
    }
}