using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace USIConnect
{
    public class Tokenizer
    {
        private int index;
        private string str;

        public Tokenizer()
        {
            this.str = null;
            this.index = 0;
        }

        public Tokenizer(string str)
        {
            this.str = str;
            this.index = 0;
        }

        public void Set(string str)
        {
            this.str = str;
            this.index = 0;
        }

        /// <summary>
        /// スペース区切りの単語抜き出し
        /// </summary>
        /// <returns></returns>
        public string Token()
        {
            string subStr = string.Empty;
            int startPos = -1;

            for (; this.index < this.str.Length; this.index++)
            {
                if (this.str[this.index] != ' ')
                {
                    if (startPos == -1)
                    {
                        startPos = this.index;
                    }
                }
                else if (this.str[this.index] == ' ')
                {
                    if (startPos != -1)
                    {
                        break;
                    }
                }
                else
                {
                    // 何もしない
                }
            }

            if (startPos != -1)
            {
                subStr = this.str.Substring(startPos, this.index - startPos);
            }

            return subStr;
        }

        /// <summary>
        /// USI optionの名前を取り出す特殊処理
        /// </summary>
        public string TokenName()
        {
            int pos;
            string subStr;

            // スペースのスキップ
            for (; this.index < this.str.Length; this.index++)
            {
                if (this.str[this.index] != ' ')
                {
                    break;
                }
            }

            if (this.index >= this.str.Length)
            {
                subStr = string.Empty;
            }
            else
            {
                pos = this.str.IndexOf(" type", this.index);
                if (pos <= this.index)
                {
                    subStr = string.Empty;
                }
                else
                {
                    subStr = this.str.Substring(this.index, pos - this.index);
                    this.index = pos + 1;
                }
            }

            return subStr;
        }

        // 残りを全部取り出す
        public string Last()
        {
            string subStr = string.Empty;

            if ((this.index + 1) < this.str.Length)
            {
                subStr = this.str.Substring(this.index + 1, this.str.Length - this.index - 1);
            }

            return subStr;
        }

        /// <summary>
        /// 数値のパース
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int ParseNum(string str, out int cnt)
        {
            int num = 0;
            int index = 0;
            bool minus = false;

            if (str.Length >= 1)
            {
                if (str[0] == '-')
                {
                    // マイナス
                    minus = true;
                    index++;
                }
            }

            for (; index < str.Length; index++)
            {
                char c = str[index];

                if (c >= '0' && c <= '9')
                {
                    num = num * 10;
                    num = num + (c - '0');
                }
                else if (c == 'K' || c == 'k')
                {
                    num = num * 1000;
                    break;
                }
                else if (c == 'M' || c == 'm')
                {
                    num = num * 1000 * 1000;
                    break;
                }
                else
                {
                    break;
                }
            }

            cnt = index;

            if (minus)
            {
                num = -num;
            }

            return num;
        }

        public static int ParseNum(string str)
        {
            int cnt;

            return ParseNum(str, out cnt);
        }

        public IEnumerable<string> Tokens
        {
            get
            {
                string str;

                while (true)
                {
                    str = this.Token();
                    if (string.IsNullOrEmpty(str))
                    {
                        break;
                    }

                    yield return str;
                }
            }
        }   
   }
}
