using System;
using System.Collections.Generic;
using System.Text;

namespace Yokinsoft.ZipCode.Data
{
    public enum KanaType
    {

        None = 0,
        Hiragana = 32,
        Katakana = 16,
        Narrow = 8,

        hankaku = Narrow,
        katakana = Katakana,
        hiragana = Hiragana

    }
}
