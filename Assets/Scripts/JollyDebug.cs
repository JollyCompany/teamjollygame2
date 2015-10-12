using UnityEngine;
using System.Collections;
using Jolly;


namespace Jolly
{
	public class JollyDebug : MonoBehaviour
	{
		// Add strings to this array to make "GetFlag", "SetFlag", "ExecuteIf", "Log*If" work with these flags.
		private string[] Flags = new string[] {
		};

		private bool[] FlagValues;

		private void InitializeSortedFlags ()
		{
			System.Array.Sort(this.Flags);
			this.FlagValues = new bool[this.Flags.Length];
			for (int i = 0; i < this.Flags.Length; ++i)
			{
				this.FlagValues[i] = false;
			}
		}

		public IEnumerator DisplayFlagsEnumerator ()
		{
			return this.Flags.GetEnumerator();
		}

		private int IndexOfFlag(string flag)
		{
			return System.Array.BinarySearch(this.Flags, flag);
		}

		public static bool GetFlag(string flag)
		{
			JollyDebug self = JollyDebug.Instance;
			int index = self.IndexOfFlag(flag);
			if (index < 0)
			{
				return false;
			}
			return !self.FlagValues[index];
		}

		public static void SetFlag(string flag, bool value)
		{
			JollyDebug self = JollyDebug.Instance;
			int index = self.IndexOfFlag(flag);
			if (index >= 0)
			{
				self.FlagValues[index] = value;
			}
		}

		private bool EvaluateFlagExpression(string flagExpression)
		{
			string[] substrings = flagExpression.Split (new char[] {' '}, System.StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < substrings.Length; ++i)
			{
				int index = this.IndexOfFlag(substrings[i]);
				JollyDebug.Assert (index > 0, "substring[{0}] == {1} not found in debug flags", i, substrings[i]);
				if (this.FlagValues[i])
				{
					return true;
				}
			}
			return false;
		}

		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void ExecuteIf(string flagExpression, System.Action callback)
		{
			JollyDebug self = JollyDebug.Instance;
			if (self.EvaluateFlagExpression(flagExpression))
			{
				callback.Invoke ();
			}
		}

		public class Expression
		{
			public string Name;
			public string LastValue;
			
			public System.Func<float> ReturnsFloat;
			public System.Func<string> ReturnsString;
			public System.Func<bool> ReturnsBool;
			
			public Expression(string name)
			{
				this.Name = name;
			}
			
			public Expression(string name, System.Func<float> returnsFloat)
			{
				this.Name = name;
				this.ReturnsFloat = returnsFloat;
			}
			
			public Expression(string name, System.Func<string> returnsString)
			{
				this.Name = name;
				this.ReturnsString = returnsString;
			}
			
			public Expression(string name, System.Func<bool> returnsBool)
			{
				this.Name = name;
				this.ReturnsBool = returnsBool;
			}
			
			public void Update()
			{
				if (null != this.ReturnsFloat)
				{
					this.SetLastValue(this.ReturnsFloat());
				}
				else if (null != this.ReturnsString)
				{
					this.SetLastValue(this.ReturnsString());
				}
				else if (null != this.ReturnsBool)
				{
					this.SetLastValue(this.ReturnsBool());
				}
			}
			
			public void SetLastValue(float floatValue)
			{
				this.LastValue = floatValue.ToString("0.00");
			}
			
			public void SetLastValue(string stringValue)
			{
				this.LastValue = stringValue;
			}
			
			public void SetLastValue(bool boolValue)
			{
				this.LastValue = boolValue.ToString();
			}
		};
		
		public class ExpressionsByOwner
		{
			public MonoBehaviour Owner;
			public ArrayList Expressions = new ArrayList();
			public bool Enabled = true;
			
			public ExpressionsByOwner(MonoBehaviour owner)
			{
				this.Owner = owner;
			}
			
			public void Add(Expression expression)
			{
				this.Expressions.Add (expression);
			}
			
			public Expression GetExpression(string name)
			{
				foreach (Expression expression in this.Expressions)
				{
					if (expression.Name.Equals(name))
					{
						return expression;
					}
				}
				Expression newExpression = new Expression(name);
				this.Add (newExpression);
				return newExpression;
			}
			
			public void Update()
			{
				if (!this.Enabled)
				{
					return;
				}
				JollyDebug.Assert(!this.OwnerIsMissing);
				foreach (Expression expression in this.Expressions)
				{
					expression.Update();
				}
			}
			
			public bool OwnerIsMissing
			{
				get
				{
					return null == this.Owner;
				}
			}
		}
		
		private ArrayList ExpressionsByOwnerList = new ArrayList();
		public IEnumerator ExpressionsByOwnerEnumerator
		{
			get
			{
				return this.ExpressionsByOwnerList.GetEnumerator();
			}
		}
		
		private static JollyDebug _instance = null;
		public static JollyDebug Instance
		{
			get
			{
				if (null != JollyDebug._instance)
				{
					return JollyDebug._instance;
				}
				
				GameObject go = GameObject.Find("JollyDebug");
				JollyDebug instance = null;
				
				if (go)
				{
					instance = go.GetComponent<JollyDebug>();
				}
				else
				{
					go = new GameObject("JollyDebug");
				}
				if (!instance)
				{
					instance = go.AddComponent<JollyDebug>() as JollyDebug;
				}
				
				JollyDebug._instance = instance;
				return instance;
			}
		}
		
		private ExpressionsByOwner GetExpressionsForOwner(MonoBehaviour owner)
		{
			foreach (ExpressionsByOwner expressionsByOwner in this.ExpressionsByOwnerList)
			{
				if (expressionsByOwner.Owner == owner)
				{
					return expressionsByOwner;
				}
			}
			ExpressionsByOwner newExpressionsByOwner = new ExpressionsByOwner(owner);
			this.ExpressionsByOwnerList.Add (newExpressionsByOwner);
			return newExpressionsByOwner;
		}
		
		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void Watch (MonoBehaviour owner, string name, System.Func<float> returnsFloat)
		{
			JollyDebug self = JollyDebug.Instance;
			self.GetExpressionsForOwner(owner).Add (new Expression(name, returnsFloat));
		}
		
		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void Watch (MonoBehaviour owner, string name, System.Func<string> returnsString)
		{
			JollyDebug self = JollyDebug.Instance;
			self.GetExpressionsForOwner(owner).Add (new Expression(name, returnsString));
		}
		
		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void Watch (MonoBehaviour owner, string name, System.Func<bool> returnsBool)
		{
			JollyDebug self = JollyDebug.Instance;
			self.GetExpressionsForOwner(owner).Add (new Expression(name, returnsBool));
		}
		
		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void Watch (MonoBehaviour owner, string name, float floatValue)
		{
			JollyDebug self = JollyDebug.Instance;
			self.GetExpressionsForOwner(owner).GetExpression(name).SetLastValue(floatValue);
		}
		
		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void Watch (MonoBehaviour owner, string name, string stringValue)
		{
			JollyDebug self = JollyDebug.Instance;
			self.GetExpressionsForOwner(owner).GetExpression(name).SetLastValue(stringValue);
		}
		
		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void Watch (MonoBehaviour owner, string name, bool boolValue)
		{
			JollyDebug self = JollyDebug.Instance;
			self.GetExpressionsForOwner(owner).GetExpression(name).SetLastValue(boolValue);
		}
		
		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void Assert (bool expression, string message = "", params object[] args)
		{
			if (expression)
			{
				return;
			}
			string linewiseStackTrace = StackTraceUtility.ExtractStackTrace();
			string firstLine = null;
			{
				string [] lines = linewiseStackTrace.Split(new char[] {'\n'});
				{
					string [] skippedFirstLine = new string [lines.Length - 1];
					System.Array.Copy(lines, 1, skippedFirstLine, 0, skippedFirstLine.Length);
					lines = skippedFirstLine;
				}
				firstLine = lines[0];
				linewiseStackTrace = string.Join("\n", lines);
			}
			message = string.Format (message, args);
			JollyDebug.LogException("ASSERTION FAILED in {0}\n{1}\n{2}", firstLine, message, linewiseStackTrace);
		}
		
		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void Log(string message, params object[] args)
		{
			string formattedMessage = string.Format(message, args);
			Debug.Log(formattedMessage);
		}
		
		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void LogWarning(string message, params object[] args)
		{
			string formattedMessage = string.Format(message, args);
			Debug.LogWarning(formattedMessage);
		}
		
		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void LogError(string message, params object[] args)
		{
			string formattedMessage = string.Format(message, args);
			Debug.LogError(formattedMessage);
		}
		
		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void LogException(string message, params object[] args)
		{
			string formattedMessage = string.Format (message, args);
			JollyDebug self = JollyDebug.Instance;
			self.ExceptionToDisplayOnScreen = formattedMessage;
			Debug.LogError(formattedMessage);
			Debug.Break ();
		}

		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void LogIf(string flagExpression, string message, params object[] args)
		{
			JollyDebug self = JollyDebug.Instance;
			if (self.EvaluateFlagExpression(flagExpression))
			{
				JollyDebug.Log (message, args);
			}
		}
		
		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void LogWarningIf(string flagExpression, string message, params object[] args)
		{
			JollyDebug self = JollyDebug.Instance;
			if (self.EvaluateFlagExpression(flagExpression))
			{
				JollyDebug.LogWarning (message, args);
			}
		}
		
		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void LogErrorIf(string flagExpression, string message, params object[] args)
		{
			JollyDebug self = JollyDebug.Instance;
			if (self.EvaluateFlagExpression(flagExpression))
			{
				JollyDebug.LogError (message, args);
			}
		}
		
		[System.Diagnostics.Conditional("DEBUG"), System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void LogExceptionIf(string flagExpression, string message, params object[] args)
		{
			JollyDebug self = JollyDebug.Instance;
			if (self.EvaluateFlagExpression(flagExpression))
			{
				JollyDebug.LogException (message, args);
			}
		}
		
		void Awake()
		{
			this.InitializeSortedFlags();
			Application.RegisterLogCallback(HandleException);
		}
		
		void OnLevelWasLoaded()
		{
			Application.RegisterLogCallback(HandleException);
		}
		
		private void HandleException(string condition, string stackTrace, LogType type)
		{
			if (type == LogType.Exception)
			{
				JollyDebug.LogException("{0}: {1}\n{2}", type, condition, stackTrace);
			}
		}
		
		void Update ()
		{
			for (int i = this.ExpressionsByOwnerList.Count - 1; i >= 0; --i)
			{
				ExpressionsByOwner expressionsByOwner = (ExpressionsByOwner)this.ExpressionsByOwnerList[i];
				if (expressionsByOwner.OwnerIsMissing)
				{
					this.ExpressionsByOwnerList.RemoveAt (i);
				}
				else
				{
					expressionsByOwner.Update();
				}
			}
		}
		
		private string ExceptionToDisplayOnScreen;
		
		void OnGUI ()
		{
			if (null == this.ExceptionToDisplayOnScreen)
			{
				return;
			}
			
			GUILayout.BeginArea (new Rect(0,0,Screen.width,Screen.height));

			GUIStyle style = new GUIStyle("textArea");
			style.normal.textColor = Color.white;
			
			GUI.Label (new Rect(Screen.width/6.0f, Screen.height/6.0f, Screen.width*2/3.0f, Screen.height*2/3.0f), this.ExceptionToDisplayOnScreen, style);
			float width = 100.0f;
			if (GUI.Button (new Rect((Screen.width - width)/2.0f, Screen.height * 5.1f / 6.0f, width, 40.0f), "Reset"))
			{
				this.ExceptionToDisplayOnScreen = null;
			}
			
			GUILayout.EndArea ();
			
		}
	}
	
}
