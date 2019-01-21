using Gameplay;
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
                text.color = new Color(0.18f, 0.49f, 0.20f, 1.0f);
                text.text = value.ToString(true);
            }
            else
            {
                text.color = new Color(0.85f, 0.26f, 0.08f, 1.0f);
                text.text = value.ToString(true);
            }
        }
    }
}