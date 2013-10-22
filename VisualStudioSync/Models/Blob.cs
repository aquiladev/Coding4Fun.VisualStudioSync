using System;

namespace VisualStudioSync.Models
{
	public class Blob
	{
		private readonly string _value;
		public string Value
		{
			get
			{
				return _value;
			}
		}

		private readonly DateTime _updated;
		public DateTime Updated
		{
			get
			{
				return _updated;
			}
		}

		public Blob (string value, DateTime updated)
		{
			_value = value;
			_updated = updated;
		}
	}
}
