using UnityEngine;
using Echoes.VN;

namespace Echoes.Narrative
{
    public static class NarrativeConditionEvaluator
    {
        public static bool Evaluate(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return true;

            condition = condition.Trim();

            if (condition.StartsWith("!"))
            {
                string flag = condition.Substring(1).Trim();
                return !GetFlag(flag);
            }

            string[] operators = { "==", "!=", ">=", "<=", ">", "<" };
            foreach (string op in operators)
            {
                int idx = condition.IndexOf(op);
                if (idx > 0)
                {
                    string left = condition.Substring(0, idx).Trim();
                    string right = condition.Substring(idx + op.Length).Trim();
                    return EvaluateComparison(left, op, right);
                }
            }

            return GetFlag(condition);
        }

        public static bool EvaluateAll(string[] conditions)
        {
            if (conditions == null || conditions.Length == 0)
                return true;

            for (int i = 0; i < conditions.Length; i++)
            {
                if (!Evaluate(conditions[i]))
                    return false;
            }
            return true;
        }

        static bool EvaluateComparison(string left, string op, string right)
        {
            float varValue = GetVariable(left);
            float rightValue = ParseOperand(right);

            if (float.IsNaN(varValue) || float.IsNaN(rightValue))
            {
                bool flagVal = GetFlag(left);
                bool rightBool = ParseBool(right);
                return op switch
                {
                    "==" => flagVal == rightBool,
                    "!=" => flagVal != rightBool,
                    _ => false
                };
            }

            return op switch
            {
                "==" => Mathf.Approximately(varValue, rightValue),
                "!=" => !Mathf.Approximately(varValue, rightValue),
                ">=" => varValue >= rightValue,
                "<=" => varValue <= rightValue,
                ">" => varValue > rightValue,
                "<" => varValue < rightValue,
                _ => false
            };
        }

        static float ParseOperand(string s)
        {
            s = s.Trim().Trim('"');
            if (float.TryParse(s, out var f))
                return f;
            return float.NaN;
        }

        static bool ParseBool(string s)
        {
            s = s.Trim().Trim('"').ToLowerInvariant();
            return s == "true" || s == "1";
        }

        static bool GetFlag(string key)
        {
            var flags = VN_EndingFlags.Instance;
            return flags != null && flags.GetFlag(key);
        }

        static float GetVariable(string name)
        {
            var ctrl = NarrativeStateController.Instance;
            if (ctrl != null && ctrl.HasVariable(name))
                return ctrl.GetVariable(name);
            return float.NaN;
        }
    }
}
