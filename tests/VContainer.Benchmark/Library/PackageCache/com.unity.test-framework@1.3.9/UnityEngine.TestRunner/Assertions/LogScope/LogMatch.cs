using System;
using System.Text.RegularExpressions;

namespace UnityEngine.TestTools.Logging
{
    [Serializable]
    internal class LogMatch
    {
        [SerializeField]
        private bool m_UseRegex;
        [SerializeField]
        private string m_Message;
        [SerializeField]
        private string m_MessageRegex;
        [SerializeField]
        private string m_LogType;

        public string Message
        {
            get { return m_Message; }
            set
            {
                m_Message = value;
                m_UseRegex = false;
            }
        }

        public Regex MessageRegex
        {
            get
            {
                if (!m_UseRegex)
                {
                    return null;
                }

                return new Regex(m_MessageRegex);
            }
            set
            {
                if (value != null)
                {
                    m_MessageRegex = value.ToString();
                    m_UseRegex = true;
                }
                else
                {
                    m_MessageRegex = null;
                    m_UseRegex = false;
                }
            }
        }

        public LogType? LogType
        {
            get
            {
                if (!string.IsNullOrEmpty(m_LogType))
                {
                    return Enum.Parse(typeof(LogType), m_LogType) as LogType ? ;
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    m_LogType = value.Value.ToString();
                }
                else
                {
                    m_LogType = null;
                }
            }
        }

        public bool Matches(LogEvent log)
        {
            if (LogType != null && LogType != log.LogType)
            {
                return false;
            }

            if (m_UseRegex)
            {
                return MessageRegex.IsMatch(log.Message);
            }

            return Message.Equals(log.Message);
        }

        public override string ToString()
        {
            if (m_UseRegex)
                return string.Format("[{0}] Regex: {1}", LogType, MessageRegex);
            return string.Format("[{0}] {1}", LogType, Message);
        }
    }
}
