// -----------------------------------------------------------------------
// <copyright file="ToolTipHelper.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Teechip.View
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Forms;

	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	public class ToolTipHelper
	{
		private static readonly Dictionary<string, ToolTip> tooltips = new Dictionary<string,ToolTip>();

		/// <summary>
		/// Constructor
		/// </summary>
		public ToolTipHelper()
		{
			//this.tooltips = new Dictionary<string, ToolTip>();
		}

		/// <summary>
		/// Key a tooltip by its control name
		/// </summary>
		/// <param name="controlName"></param>
		/// <returns></returns>
		public static ToolTip GetControlToolTip(string controlName)
		{
			if (tooltips.ContainsKey(controlName))
			{
				return tooltips[controlName];
			}
			else
			{
				ToolTip tt = new ToolTip();
				tooltips.Add(controlName, tt);
				return tt;
			}
		}

		public static void SetToolTip(Control control, string text)
		{
			if (String.IsNullOrEmpty(text))
			{
				if (tooltips.ContainsKey(control.Name))
				{
					GetControlToolTip(control.Name).RemoveAll();
					tooltips.Remove(control.Name);
				}
			}
			else
			{
				ToolTip tt = GetControlToolTip(control.Name);
				tt.SetToolTip(control, text);
			}
		}
	}
}
