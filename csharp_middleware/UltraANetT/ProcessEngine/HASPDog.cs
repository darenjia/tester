using Aladdin.HASP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ProcessEngine
{
    public class HASPDog
    {
        public static bool IsActivate()
        {
            HaspFeature feature = HaspFeature.FromFeature(0);
            string vendorCode =
            "QyHtqY0sg5cNr3gLflwugWX4JCxR7QQs23wBaNOa/8F6PjcbjIWn5YjwXVpIrE7c8DuX7Mx1dWVIJUhI" +
            "jV3qUl4aBXhR8TZm1cMWV+SaqyRDS1iEbStM6wrWnkwzxQly/8W+KZgt38JvPbdPqi1ohm6WOnlEmOvU" +
            "DE0omLx6GCrkfxRaitO0TBkxIroSeMcauoiZn2XD373gGp4R0zS6qh6o33t6eQT7K6x+t43X8Yjp86vU" +
            "sPaVa6Q2xiWhaNOcqNhLd6CWgEGzhOxsmRPj0pY0/Cuvf+xU/AEQ/JtbgipetakVvmbg3iroTO9GLmCK" +
            "KaGj9T9rn6PZr8tbhj8Xc2m3zhbLlv/5CN43UC1lbw9jAkBMyAFVxtRlg8vVW0Oat8fzFIKrHKmDnVI9" +
            "wql+kG/RiDjjEa85cKDju1igsoDIhDvvlcOPgXV8j66QAYq0CiCCFqLFk5uJ3zAsledgkE8maBQgcwFA" +
            "o5ZrA/4GWm+3rDnBZwXtM+IOauXqb6yXP/K+sWtZ8C/dppKyivq0XpjV2PMfRvbHiOoaDXvM3OYRJ+3G" +
            "ipVetKGP5lAIQBpqmJ9/yS/tIadKXw4EW0I4nAJRipJ2UCMwmCpa11NNhyCaiTRjQfX0gbrhHpH7m+Zh" +
            "jT15tgrbPHhgdARDx/HDM0xHAiXeMG2iQSNfrUndkZUxy+QctGgAhxG/pee++kQV31JPxqvcORWsRsQ1" +
            "KZHB7V2viLCNL50N6H4mbvJMEeBDVSrHe7nyN3zwljvb6YoZ6wrZmXIGEujL3EObZhyT9TwtsZ+jvmwC" +
            "vaezc+YIeSbddbdhNG7yqfRpgoDEiIsEGlznqBKyBAovtLiyvdtEiWIsmrmZugCKr4xMtDt48LynPTL7" +
            "38tFCj5nWZi6vWZsmRk+Cx6fgNE4ujbYWxKITA0j+4S48il5EC4XkvNldJ06tBCZdOCoSg+2CGjIAtyu" +
            "eD/Rk30FtOhwA+32vB6Etg==";
            Hasp hasp = new Hasp(feature);
            HaspStatus status = hasp.Login(vendorCode);
            if (HaspStatus.StatusOk != status)
            {
                return false;
            }
            return true;
        }
    }

    

}
