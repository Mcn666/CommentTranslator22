using System;
using System.Collections.Generic;
using System.Text;

namespace Dictionary
{
    /// <summary>
    /// 这些字段对应json中的值
    /// </summary>
    public class DictionaryResultFormat
    {
        public string en { get; set; } = string.Empty;

        public string zh { get; set; } = string.Empty;

        public string ja { get; set; } = string.Empty;
    }
}
