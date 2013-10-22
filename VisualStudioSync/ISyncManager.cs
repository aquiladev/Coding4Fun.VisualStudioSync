using System;

namespace VisualStudioSync
{
	public interface ISyncManager
	{
		void Sync(DateTime? updated);
	}
}
