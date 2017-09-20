// -----------------------------------------------------------------------
// <copyright file="ComboboxItem.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Teechip.View
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	public class ComboboxItem
	{
        //public ComboboxItem(object value)
        //{
        //    Text = value.ToString();
        //    Value = value;
        //}

        //public ComboboxItem()
        //{
        //}

		public string Text { get; set; }
		public object Value { get; set; }

		public override string ToString()
		{
			return Text;
		}
	}
}
